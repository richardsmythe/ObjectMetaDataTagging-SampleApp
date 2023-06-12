﻿using ObjectMetaDataTagging.Extensions;
using ObjectMetaDataTagging.Models;

namespace ObjectMetaDataTagging
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Mechanism for tagging meta data to objects
            // - Event driven: can trigger events based on needs
            // - Multiple tags: each object can have multiple tags

            // To do: Querying tags 

            Console.WriteLine("####### Test for event triggered transaction #######");
            PersonTransaction trans1 = new PersonTransaction { Sender = "John", Receiver = "Richard", Amount = 1433.00};

            trans1.SetTag("Payment");

            var allTags = trans1.GetAllTags();
            foreach (var tag in allTags)
            {
                Console.WriteLine($"Tag: {tag.Key}, Value: {tag.Value}");
            }

            Console.WriteLine("\n####### Test for non-event triggered transaction #######");
            PersonTransaction trans2 = new PersonTransaction { Sender = "John", Receiver = "Richard", Amount = 588.50 };

            trans2.SetTag("Payment");

            allTags = trans2.GetAllTags();
            foreach (var tag in allTags)
            {
                Console.WriteLine($"Tag: {tag.Key}, Value: {tag.Value}");
            }


            Console.Read();
        }
    }  
}