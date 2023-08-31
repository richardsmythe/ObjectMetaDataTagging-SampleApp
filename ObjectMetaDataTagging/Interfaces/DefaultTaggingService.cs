using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Models;
using System.Collections.Concurrent;

namespace ObjectMetaDataTagging.Interfaces
{
    public class DefaultTaggingService : IDefaultTaggingService
    {
        private readonly ConcurrentDictionary<WeakReference, Dictionary<Guid, BaseTag>> data = new ConcurrentDictionary<WeakReference, Dictionary<Guid, BaseTag>>();
        private readonly TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs> _eventManager;

        public DefaultTaggingService(TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs> eventManager)
        {
            _eventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
        }

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
        public virtual IEnumerable<BaseTag> GetAllTags(object o)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null && data.TryGetValue(key, out var tags))
            {
                return tags.Values.ToList();
            }
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


        public virtual void RemoveAllTags(object o)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null)
            {
                data.Remove(key, out _);
            }
        }

        public virtual bool RemoveTag(object o, Guid tagId)
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
            if (tag == null) return;

            if (o != null)
            {
                tag.AssociatedParentObjectName = o.GetType().Name;
            }

            var weakRef = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (weakRef == null)
            {
                weakRef = new WeakReference(o);
            }

            var tagDictionary = data.GetOrAdd(weakRef, new Dictionary<Guid, BaseTag>());
            lock (tagDictionary)
            {
                tagDictionary[tag.Id] = tag;
                //Console.WriteLine($"Tag Dictionary after addition: {string.Join(", ", tagDictionary.Select(kvp => kvp.Value.Name))}");
            }

            _eventManager.RaiseTagAdded(new TagAddedEventArgs(o, tag));
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

    }
}
