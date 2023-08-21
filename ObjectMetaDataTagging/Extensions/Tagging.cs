using ObjectMetaDataTagging.Models;
using System.Collections.Concurrent;
using System.Linq;

namespace ObjectMetaDataTagging.Extensions
{
    public static class ObjectTaggingExtensions
    {
        private static ConcurrentDictionary<WeakReference, List<object>> data = new ConcurrentDictionary<WeakReference, List<object>>();

        private static readonly TaggingEventManager _eventManager = new TaggingEventManager();

        public static event EventHandler<TagAddedEventArgs> TagAdded
        {
            add => _eventManager.TagAdded += value;
            remove => _eventManager.TagAdded -= value;
        }

        public static T? GetTag<T>(this object o, int tagIndex)
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


        public static bool HasTag<T>(this object o, T tag)
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

        public static void SetTag<T>(this object o, T tag)
        {
            // try to find the existing weak reference
            var weakRef = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (weakRef == null)
            {
                weakRef = new WeakReference(o);
            }

            var tagList = data.GetOrAdd(weakRef, new List<object>());

            lock (tagList)  // only one thread can update this specific list at a time
            {
                if (!tagList.Contains(tag))
                {
                    tagList.Add(tag);
                }
            }

            _eventManager.RaiseTagAdded(new TagAddedEventArgs(o, tag));
        }

        public static void RemoveAllTags(this object o)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null)
            {
                data.Remove(key, out _);
            }
        }

        public static IEnumerable<KeyValuePair<string, object>> GetAllTags(this object o)
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

        public static List<IEnumerable<KeyValuePair<string, object>>> GenerateTestData()
        {
            var testData = new List<IEnumerable<KeyValuePair<string, object>>>();

            var trans1 = new PersonTransaction { Sender = "John", Receiver = "Richard", Amount = 1433.00 };
            trans1.SetTag(Tags.FundsTransfer);
            trans1.SetTag(Tags.Suspicious);
            trans1.SetTag(Tags.PaymentExpired);
            testData.Add(trans1.GetAllTags().ToList());

            var trans2 = new PersonTransaction { Sender = "Greg", Receiver = "Tom", Amount = 355.00 };
            trans2.SetTag(Tags.PaymentExpired);
            testData.Add(trans2.GetAllTags().ToList());

            //var trans3 = new PersonTransaction { Sender = "Sam", Receiver = "James", Amount = 31.50 };
            //trans3.SetTag(Tags.FundsTransfer);
            //testData.Add(trans3.GetAllTags().ToList());

            //var trans4 = new PersonTransaction { Sender = "Adam", Receiver = "Sue", Amount = 5000.52 };
            //trans4.SetTag(Tags.FundsTransfer);
            //testData.Add(trans4.GetAllTags().ToList()); 

            return testData;
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
