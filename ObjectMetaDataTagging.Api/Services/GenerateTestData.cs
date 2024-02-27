﻿using ObjectMetaDataTagging.Api.Interfaces;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Api.Services
{
    public class GenerateTestData : IGenerateTestData
    {
        private readonly ITaggingManager<BaseTag> _taggingManager;
        private readonly ITagFactory _tagFactory;

        public GenerateTestData(
            ITaggingManager<BaseTag> taggingManager,
            ITagFactory tagFactory)
        {
            _taggingManager = taggingManager;
            _tagFactory = tagFactory;
        }

        // Generates 3 levels of tags on objects
        async Task<List<IEnumerable<KeyValuePair<string, object>>>> IGenerateTestData.GenerateTestData()
        {
            var testData = new List<IEnumerable<KeyValuePair<string, object>>>();
            var random = new Random();
            int numberOfObjects = random.Next(1, 1);
            var dummyClasses = new List<Type> { typeof(Transaction), typeof(Fraud), typeof(Address) };

            for (int i = 0; i < numberOfObjects; i++)
            {
                var selectedClassType = dummyClasses[random.Next(dummyClasses.Count)];
                var newObj = Activator.CreateInstance(selectedClassType) as DummyBase;

                newObj.Sender = "Sender" + random.Next(1, 50);
                newObj.Receiver = "Receiver" + random.Next(1, 50);
                newObj.Amount = random.Next(4500, 6000);

                int numberOfTags = random.Next(1, 4);
                var tagTypes = Enum.GetValues(typeof(ExampleTags)).Cast<ExampleTags>().ToArray();

                for (int j = 0; j < numberOfTags; j++)
                {
                    var randomTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                    BaseTag newTag = _tagFactory.CreateBaseTag(randomTagName, null, "");
                    await _taggingManager.SetTagAsync(newObj, newTag);

                    int numberOfChildTags = random.Next(1, 1);

                    for (int k = 0; k < numberOfChildTags; k++)
                    {
                        var randomChildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                        var childTag = _tagFactory.CreateBaseTag(randomChildTagName, null, $"Child tag {k + 1}");

                        childTag.Parents.Add(newTag.Id);
                        childTag.Value = $"Child Value {k + 1}";

                        int numberOfGrandchildTags = random.Next(1, 1);

                        for (int m = 0; m < numberOfGrandchildTags; m++)
                        {
                            var randomGrandchildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                            var grandchildTag = _tagFactory.CreateBaseTag(randomGrandchildTagName, null, $"Grandchild tag {m + 1}");

                            grandchildTag.Parents.Add(childTag.Id);
                            grandchildTag.Value = $"Grandchild Value {m + 1}";

                            childTag.AddChildTag(grandchildTag);
                        }

                        newTag.AddChildTag(childTag);
                    }

                    var tags = await _taggingManager.GetAllTags(newObj);
                    testData.Add(tags.Select(tag => new KeyValuePair<string, object>(tag.Name, tag)).ToList());
                }
            }

            return testData;
        }

        public class DummyBase
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Sender { get; set; }
            public string Receiver { get; set; }
            public int Amount { get; set; }
        }

        public class Fraud : DummyBase
        {
        }

        public class Address : DummyBase
        {    
        }

        public class Transaction : DummyBase
        {        

        }

    }
}