using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ObjectMetaDataTagging.Api.Interfaces;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Helpers;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Api.Services
{
    public class GenerateTestData : IGenerateTestData
    {
        private readonly ITaggingManager<BaseTag> _taggingManager;
        private readonly HashSet<Guid> handledObjectIds;

        public GenerateTestData(ITaggingManager<BaseTag> taggingService)
        {
            _taggingManager = taggingService ?? throw new ArgumentNullException(nameof(taggingService));
            handledObjectIds = new HashSet<Guid>();

            // Shows how external components can subscribe to events in the library and perform certain actions
            _taggingManager.TagAdded += async (sender, args) =>
            {
                if (args.TaggedObject is DummyBase dummyObject)
                {
                    if (!handledObjectIds.Contains(dummyObject.Id))
                    {
                        handledObjectIds.Add(dummyObject.Id);
                        if (dummyObject.Amount > 2000)
                        {                           
                            var newTag = _taggingManager.CreateBaseTag("Suspicious Transfer Detected", ExampleTags.Suspicious, "This object has been tagged as suspicious");
                            newTag.Parents.Add(dummyObject.Id);
                            await _taggingManager.SetTagAsync(dummyObject, newTag);
                        }
                    }
                }
            };
        }

        // Generates 3 levels of tags on objects
        async Task<List<IEnumerable<KeyValuePair<string, object>>>> IGenerateTestData.GenerateTestData()
        {
            
            var testData = new List<IEnumerable<KeyValuePair<string, object>>>();
            var random = new Random();
            int numberOfObjects = random.Next(1, 3);

            var dummyClasses = new List<Type> { typeof(Transaction), typeof(Fraud), typeof(Address) };

            for (int i = 0; i < numberOfObjects; i++)
            {
                var selectedClassType = dummyClasses[random.Next(dummyClasses.Count)];
                var newObj = Activator.CreateInstance(selectedClassType) as DummyBase;

                newObj.Sender = "Sender" + random.Next(1, 50);
                newObj.Receiver = "Receiver" + random.Next(1, 50);
                newObj.Amount = random.Next(1500, 4000);

                int numberOfTags = random.Next(1, 4);
                var tagTypes = Enum.GetValues(typeof(ExampleTags)).Cast<ExampleTags>().ToArray();

                for (int j = 0; j < numberOfTags; j++)
                {
                    var randomTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                    BaseTag newTag = _taggingManager.CreateBaseTag(randomTagName, null, "");
                    await _taggingManager.SetTagAsync(newObj, newTag);

                    int numberOfChildTags = random.Next(1, 5);

                    for (int k = 0; k < numberOfChildTags; k++)
                    {
                        var randomChildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                        var childTag = _taggingManager.CreateBaseTag(randomChildTagName, null, $"Child tag {k + 1}");

                        childTag.Parents.Add(newTag.Id);
                        childTag.Value = $"Child Value {k + 1}";

                        int numberOfGrandchildTags = random.Next(1, 3);

                        for (int m = 0; m < numberOfGrandchildTags; m++)
                        {
                            var randomGrandchildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                            var grandchildTag = _taggingManager.CreateBaseTag(randomGrandchildTagName, null, $"Grandchild tag {m + 1}");

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
    }
}
