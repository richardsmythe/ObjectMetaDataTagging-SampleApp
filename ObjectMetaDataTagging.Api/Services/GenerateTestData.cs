using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ObjectMetaDataTagging.Api.Interfaces;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Api.Services
{
    public class GenerateTestData : IGenerateTestData
    {
        private readonly IObjectMetaDataTaggingFacade<BaseTag> _taggingFacade;

        public GenerateTestData(
            IObjectMetaDataTaggingFacade<BaseTag> taggingService)
        {
            _taggingFacade = taggingService;            
            _taggingFacade.TagAdded += (sender, args) =>
            {
                Console.WriteLine($"Tag added for object {args.TaggedObject.GetType().Name}: {args.Tag}");
            };
        }

        // Generates 3 levels of tags on objects
        async Task<List<IEnumerable<KeyValuePair<string, object>>>> IGenerateTestData.GenerateTestData()
        {
            
            var testData = new List<IEnumerable<KeyValuePair<string, object>>>();
            var random = new Random();
            int numberOfObjects = random.Next(3, 5);

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
                    BaseTag newTag = _taggingFacade.CreateBaseTag(randomTagName, null, "");
                    await _taggingFacade.SetTagAsync(newObj, newTag);

                    int numberOfChildTags = random.Next(1, 5);

                    for (int k = 0; k < numberOfChildTags; k++)
                    {
                        var randomChildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                        var childTag = _taggingFacade.CreateBaseTag(randomChildTagName, null, $"Child tag {k + 1}");

                        childTag.Parents.Add(newTag.Id);
                        childTag.Value = $"Child Value {k + 1}";

                        int numberOfGrandchildTags = random.Next(1, 3);

                        for (int m = 0; m < numberOfGrandchildTags; m++)
                        {
                            var randomGrandchildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                            var grandchildTag = _taggingFacade.CreateBaseTag(randomGrandchildTagName, null, $"Grandchild tag {m + 1}");

                            grandchildTag.Parents.Add(childTag.Id);
                            grandchildTag.Value = $"Grandchild Value {m + 1}";

                            childTag.AddChildTag(grandchildTag);
                        }

                        newTag.AddChildTag(childTag);
                    }

                    var tags = await _taggingFacade.GetAllTags(newObj);
                    testData.Add(tags.Select(tag => new KeyValuePair<string, object>(tag.Name, tag)).ToList());
                }
            }

            return testData;
        }
    }
}
