using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Models.TagModels;
using System.Collections.Concurrent;
using System.Security.AccessControl;
using System.Transactions;

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

        // using object instead of weakreference, make sure to GC
        protected readonly ConcurrentDictionary<object, Dictionary<Guid, BaseTag>> data = new ConcurrentDictionary<object, Dictionary<Guid, BaseTag>>();


        private readonly TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs> _eventManager;

        /* By exposing these events, it allow consumers to attach event handlers 
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

            if (o != null && data.TryGetValue(o, out var tags))
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

            if (data.TryGetValue(o, out var tagDictionary))
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

            if (o != null)
            {
                data.Remove(o, out _);
            }
        }

        public virtual bool RemoveTag(object? o, Guid tagId)
        {

            if (o != null && data.TryGetValue(o, out var tags))
            {

                lock (tags)
                {
                    if (tags.Remove(tagId))
                    {
                        // if there are no tags left for this object, remove entry
                        if (tags.Count == 0)
                        {
                            data.TryRemove(o, out _);
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
            var objectId = GetObjectId(o);
            var tagDictionary = data.GetOrAdd(o, new Dictionary<Guid, BaseTag>());

            lock (tagDictionary)
            {
                var tagFromEvent = _eventManager.RaiseTagAdded(new TagAddedEventArgs(o, tag)) ?? tag;

                tagFromEvent.AssociatedParentObjectName = objectName;
                tagFromEvent.AssociatedParentObjectId = objectId;
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

            if (o == null) return false;

            modifiedTag.DateLastUpdated = DateTime.UtcNow;

            if (data.TryGetValue(o, out var tags))
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

            if (o != null && data.TryGetValue(o, out var tags))
            {
                lock (tags)
                {
                    return tags.ContainsKey(tagId);
                }
            }
            return false;
        }

        public virtual object? GetObjectByTag(Guid tagId)
        { // is this returning the correct object?????
            foreach (var kvp in data)
            {
                var tags = kvp.Value;

                lock (tags)
                {
                    if (tags.TryGetValue(tagId, out var tag))
                    {
                        return kvp.Key; // kvp.Key is now the associated object
                    }
                }
            }

            return null;
        }

        private Guid GetObjectId(object o)
        {
            if (o != null)
            {
                var idProperty = o.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    var idValue = idProperty.GetValue(o);
                    if (idValue is Guid guidValue)
                    {
                        return guidValue;
                    }
                }
            }
            return Guid.Empty;
        }


        #endregion



    }
}
