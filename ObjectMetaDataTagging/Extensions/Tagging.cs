namespace ObjectMetaDataTagging.Extensions
{
    public static class ObjectTaggingExtensions
    {
        private static Dictionary<WeakReference, List<object>> data = new Dictionary<WeakReference, List<object>>();
        private static readonly TaggingEventManager _eventManager = new TaggingEventManager();

        public static event EventHandler<TagAddedEventArgs> TagAdded
        {
            add => _eventManager.TagAdded += value;
            remove => _eventManager.TagAdded -= value;
        }

        public static T? GetTag<T>(this object o, int tagIndex)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null && data[key].Count > tagIndex && data[key][tagIndex] is T)
            {
                return (T)data[key][tagIndex];
            }
            else
            {
                return default;
            }
        }

        public static bool HasTag<T>(this object o, T tag)
        {
            // true if the object has the specificied tag already attached to it
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null)
            {
                var existingTags = data[key];
                // checks t is T
                return existingTags.Any(t => t is T 
                    && EqualityComparer<T>.Default.Equals((T)t, tag));
            }

            return false;
        }

        public static void SetTag<T>(this object o, T tag)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null && tag != null)
            {
                var existingTags = data[key];
                if (!existingTags.Contains(tag))
                {
                    // tag is added to the existing list
                    existingTags.Add(tag);
                    _eventManager.RaiseTagAdded(new TagAddedEventArgs(o, tag));
                }
            }
            else
            {
                // A new key value pair is added to the dict
                data.Add(new WeakReference(o), new List<object> { tag! });
                _eventManager.RaiseTagAdded(new TagAddedEventArgs(o, tag));
            }
        }

        public static void RemoveAllTags(this object o)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null)
            {
                data.Remove(key);
            }
        }

        public static IEnumerable<KeyValuePair<string, object>> GetAllTags(this object o)
        {
            var tags = new List<KeyValuePair<string, object>>();
            var keys = data.Keys.Where(k => k.IsAlive && k.Target == o);

            foreach (var key in keys)
            {
                var values = data[key];
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

        public class TagAddedEventArgs : EventArgs
        {
            public object Object { get; set; }
            public object Tag { get; set; }

            public TagAddedEventArgs(object obj, object tag)
            {
                Object = obj;
                Tag = tag;
            }
        }
    }
}
