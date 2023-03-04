using System.Globalization;
using System.Security.Cryptography;

namespace ObjectMetaDataTagging
{
    // A flexible mechanism for associating metadata with object instances.

    public class Program { 
        static void Main(string[] args)
        {
            // To do: Querying tags, event driven tagging, allowing multiple tags per Set, 

            string s = "I am a string object";      

            s.SetTag("foo");
            s.SetTag("bar");
            s.SetTag(12);

            var allTags = s.GetAllTags();
            foreach (var tag in allTags)
            {
                Console.WriteLine(tag);
            }
        }
    }

    public static class ObjectTaggingExtensions
    {
        private static Dictionary<WeakReference, List<object>> data
           = new Dictionary<WeakReference, List<object>>();

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

        public static void SetTag<T>(this object o, T tag)
        {
            var key = data.Keys.FirstOrDefault(k => k.IsAlive && k.Target == o);
            if (key != null)
            {
                data[key].Add(tag);
            }
            else
            {
                data.Add(new WeakReference(o), new List<object> { tag });
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

            // E.g:
            // [System.String, [System.String, foo]]
            // [System.String, [System.Int32, 3]]
            foreach (var key in keys)
            {
                var values = data[key];
                foreach (var value in values)
                {
                    if (value is KeyValuePair<string, object> tag)
                    {
                        tags.Add(new KeyValuePair<string, object>(o.GetType().ToString(), tag));
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


    }


}