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

            Console.WriteLine("\nShow all tags: "  );
            var allTags = s.GetAllTags();
            foreach (var tag in allTags)
            {
                Console.WriteLine(tag);
            }

            // This will trigger a transaction event
            Console.WriteLine("####### Test for event driven tags #######");
            PersonTransaction trans1 = new PersonTransaction { Sender = "John", Receiver = "Richard", Amount = 1433.00};

            trans1.SetTag("Payment");

            var tags = trans1.GetAllTags();
            foreach (var tag in tags)
            {
                Console.WriteLine($"Tag: {tag.Key}, Value: {tag.Value}");
            }


            Console.Read();
        }
    }  
}