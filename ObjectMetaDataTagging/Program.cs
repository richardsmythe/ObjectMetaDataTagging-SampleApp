using System.Globalization;
using System.Security.Cryptography;

namespace ObjectMetaDataTagging
{
    // A flexible mechanism for associating metadata with object instances
    public class Program
    {
        static void Main(string[] args)
        {
            // TODO : chaining like s.SetTag("cheese").SetTag("lego")
            string s = "I am a string object";
            
            s.SetTag("cheese");
            s.SetTag("lego");
            s.SetTag(1234);
            
            var allTags = s.GetAllTags();
            foreach (var tag in allTags)
            {
                Console.WriteLine(tag);
            }

            s.RemoveAllTags();

            s.SetTag("potato");
            s.SetTag("cucumber");

            var tag1 = s.GetTag<string>(0);
            Console.WriteLine(tag1);
            var tag2 = s.GetTag<string>(1);
            Console.WriteLine(tag2);

        }
    }

    public static class ObjectTaggingExtensionMethod
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

        public static IEnumerable<object> GetAllTags(this object o)
        {
            var tags = new List<object>();
            var keys = data.Keys.Where(k => k.IsAlive && k.Target == o);
            foreach (var key in keys)
            {
                tags.AddRange(data[key]);
            }
            return tags;
        }

    }


}