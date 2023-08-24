using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.NewFolder;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using static ObjectMetaDataTagging.Extensions.ObjectTaggingExtensions;

namespace ObjectMetaDataTagging.Interfaces
{
    public class DefaultTaggingService : IDefaultTaggingService
    {
        private readonly ConcurrentDictionary<WeakReference, List<object>> data = new ConcurrentDictionary<WeakReference, List<object>>();
        private readonly TaggingEventManager _eventManager;

        public DefaultTaggingService(IAlertService alertService, TaggingEventManager eventManager)
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
        public virtual IEnumerable<KeyValuePair<string, object>> GetAllTags(object o)
        {
            var tags = new List<KeyValuePair<string, object>>();
            var keys = data.Keys.Where(k => k.IsAlive && k.Target == o);

            foreach (var key in keys)
            {
                List<object> values;
                lock (data[key])
                {
                    values = new List<object>(data[key]); // create a copy while under lock
                }

                foreach (var value in values)
                {
                    if (value is KeyValuePair<string, object> tag)
                    {
                        tags.Add(tag);
                    }
                    else
                    {
                        tags.Add(new KeyValuePair<string, object>(o.GetType().ToString(),
                            new KeyValuePair<string, object>(value.GetType().ToString(), value)));
                    }
                }
            }
            return tags;
        }

        public virtual T? GetTag<T>(object o, int tagIndex)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null && data.TryGetValue(key, out var tagList))
            {
                lock (tagList)
                {
                    if (tagList.Count > tagIndex && tagList[tagIndex] is T)
                    {
                        return (T)tagList[tagIndex];
                    }
                }
            }
            return default;
        }

        public virtual bool HasTag<T>(object o, T tag)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null && data.TryGetValue(key, out var tagList))
            {
                lock (tagList)
                {
                    return tagList.Any(t => t is T && EqualityComparer<T>.Default.Equals((T)t, tag));
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

        public virtual void RemoveTag(object o, int tagIndex)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null && data.TryGetValue(key, out var tagList))
            {
                lock (tagList)
                {
                    if (tagList.Count > tagIndex)
                    {
                        tagList.RemoveAt(tagIndex);
                    }

                    // if no tags are left, remove the key from the dictionary
                    if (tagList.Count == 0)
                    {
                        data.TryRemove(key, out _);
                    }
                }
            _eventManager.RaiseTagRemoved(new TagRemovedEventArgs(o, tagIndex));
            }
        }

        public virtual void SetTag<T>(object o, T tag)
        {
            if (tag == null) return;

            // try to find the existing weak reference
            var weakRef = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (weakRef == null)
            {
                weakRef = new WeakReference(o);
            }

            var tagList = data.GetOrAdd(weakRef, new List<object>());

            lock (tagList) 
            {
                if (!tagList.Contains(tag))
                {
                    tagList.Add(tag);
                }
            }

            _eventManager.RaiseTagAdded(new TagAddedEventArgs(o, tag));
        }

        public bool UpdateTag<T>(object o, T oldTag, T newTag)
        {
            if (oldTag == null || newTag == null) return false;
            var weakRef = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (weakRef == null) return false;
            if (data.TryGetValue(weakRef, out var tagList))
            {
                lock (tagList)
                {
                    int index = tagList.IndexOf(oldTag);
                    if (index < 0) return false;
                    tagList[index] = newTag;
     
                    _eventManager.RaiseTagUpdated(new TagUpdatedEventArgs(o,oldTag));

                    return true;

                }
            }
            return false;
        }
    }
}
