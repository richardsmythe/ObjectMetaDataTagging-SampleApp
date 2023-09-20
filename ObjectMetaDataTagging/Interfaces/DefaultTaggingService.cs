using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Models.TagModels;
using System.Collections.Concurrent;
using System.Security.AccessControl;

namespace ObjectMetaDataTagging.Interfaces
{

    public class DefaultTaggingService : IDefaultTaggingService
    {
        /// <summary>
        /// Constructs a DefaultTaggingService with the specified TaggingEventManager for event handling.
        /// </summary>
        /// <param name="eventManager">The TaggingEventManager used for handling tagging-related events.</param>
        public DefaultTaggingService(
            TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs> eventManager)
        {
            _eventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
        }

        protected readonly ConcurrentDictionary<WeakReference, Dictionary<Guid, BaseTag>> data = new ConcurrentDictionary<WeakReference, Dictionary<Guid, BaseTag>>();
        private readonly TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs> _eventManager;

        /* By exposing these events, it allow consumers to attach event handlers to these events 
         * to perform additional actions when tags are added, removed, or updated. 
         * This can be useful if someone wants to extend the behavior of the library. */

        public event EventHandler<TagAddedEventArgs> TagAdded
        {
            add => _eventManager.TagAdded += value;
            remove => _eventManager.TagAdded -= value;
        }

        public event EventHandler<TagRemovedEventArgs> TagRemoved
        {
            add => _eventManager.TagRemoved += value;
            remove => _eventManager.TagRemoved -= value;
        }

        public event EventHandler<TagUpdatedEventArgs> TagUpdated
        {
            add => _eventManager.TagUpdated += value;
            remove => _eventManager.TagUpdated -= value;
        }

        /*  Methods are virtual so defualt behaviour can be override. 
         *  Strategy pattern, where behaviour is encapsulated in 
         *  separate strategy objects rather than overridden methods.        
         */
        #region Default Tag Operations
        public virtual IEnumerable<BaseTag> GetAllTags(object o)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null && data.TryGetValue(key, out var tags))
            {
                var allTags = tags.Values.ToList();
                Console.WriteLine("Tags:");
                foreach (var tag in allTags)
                {
                    Console.WriteLine($"- Tag Id: {tag.Id}, Name: {tag.Name}, Value: {tag.Value}");
                }
                return allTags;
            }
            Console.WriteLine("No tags found for the object.");
            return Enumerable.Empty<BaseTag>();
        }

        public virtual BaseTag? GetTag(object o, Guid tagId)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null && data.TryGetValue(key, out var tagDictionary))
            {
                if (tagDictionary.TryGetValue(tagId, out var tag))
                {
                    return tag;
                }
            }
            return null;
        }


        public virtual void RemoveAllTags(object o)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null)
            {
                data.Remove(key, out _);
            }
        }

        public virtual bool RemoveTag(object? o, Guid tagId)
        {             
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null && data.TryGetValue(key, out var tags))
            {
                
                lock (tags)
                {
                    if (tags.Remove(tagId))
                    {
                        // if there are no tags left for this object, remove entry
                        if (tags.Count == 0)
                        {
                            data.TryRemove(key, out _);
                        }

                        _eventManager.RaiseTagRemoved(new TagRemovedEventArgs(o, tagId));
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void SetTag(object o, BaseTag tag)
        {
            if (tag == null || o == null) return;

            var objectName = o.GetType().Name;

            var weakRef = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (weakRef == null)
            {
                weakRef = new WeakReference(o);
            }

            var tagDictionary = data.GetOrAdd(weakRef, new Dictionary<Guid, BaseTag>());

            lock (tagDictionary)
            {
                var tagFromEvent = _eventManager.RaiseTagAdded(new TagAddedEventArgs(o, tag)) ?? tag;

                tagFromEvent.AssociatedParentObjectName = objectName;
                if (tagFromEvent != null)
                {
                    tagDictionary[tagFromEvent.Id] = tagFromEvent;
                    tagDictionary[tag.Id] = tag;
                }
            }
        }

        public bool UpdateTag(object o, Guid tagId, BaseTag modifiedTag)
        {
            if (modifiedTag == null) return false;
            var weakRef = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (weakRef == null) return false;

            modifiedTag.DateLastUpdated = DateTime.UtcNow;

            if (data.TryGetValue(weakRef, out var tags))
            {
                lock (tags)
                {
                    if (tags.ContainsKey(tagId))
                    {
                        var oldTag = tags[tagId];

                        tags[tagId] = modifiedTag;
                        _eventManager.RaiseTagUpdated(new TagUpdatedEventArgs(o, oldTag, modifiedTag));

                        return true;
                    }
                }
            }
            return false;
        }
        public bool HasTag(object o, Guid tagId)
        {
            var weakRef = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (weakRef != null && data.TryGetValue(weakRef, out var tags))
            {
                lock (tags)
                {
                    return tags.ContainsKey(tagId);
                }
            }
            return false;
        }

        public virtual object? GetObjectByTag(Guid tagId)
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var tags = kvp.Value;

                lock (tags)
                {
                    if (tags.ContainsKey(tagId))
                    {
                        var associatedObject = key.Target; 

                        if (associatedObject != null)
                        {
                            return associatedObject;
                        }
                    }
                }
            }

            return null; 
        }



        #endregion



    }
}
