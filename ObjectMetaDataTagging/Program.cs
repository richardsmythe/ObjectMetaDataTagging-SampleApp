using ObjectMetaDataTagging.Extensions;
using ObjectMetaDataTagging.Models;

namespace ObjectMetaDataTagging
{
    public class Program
    {
        static void Main(string[] args)
        {
            // To do: Querying tags, event driven tagging, allowing multiple tags per Set, 

            string s = "I am a string object";
            s.SetTag("foo");
            s.SetTag("bar");
            s.SetTag(12);

            //var allTags = s.GetAllTags();
            //foreach (var tag in allTags)
            //{
            //    Console.WriteLine(tag);
            //}

            Person p = new Person { Name = "John", Age = 30 };
            p.SetTag("Suspicious");

            //foreach (var tag in p.GetAllTags())
            //{
            //    Console.WriteLine(tag);
            //}

            Console.Read();
        }
    }  
}