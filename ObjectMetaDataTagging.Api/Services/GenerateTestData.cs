using ObjectMetaDataTagging.Api.Interfaces;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Api.Services
{
    public class GenerateTestData : IGenerateTestData
    {
        private readonly IDefaultTaggingService<BaseTag> _taggingService;
        private readonly ITagFactory _tagFactory;

        public GenerateTestData(
            IDefaultTaggingService<BaseTag> taggingService,
            ITagFactory tagFactory)
        {
            _taggingService = taggingService;
            _tagFactory = tagFactory;          
        }
        async Task<List<IEnumerable<KeyValuePair<string, object>>>> IGenerateTestData.GenerateTestData()
        {
            var testData = new List<IEnumerable<KeyValuePair<string, object>>>();
            var random = new Random();
            int numberOfObjects = random.Next(3, 3);

            var dummyClasses = new List<Type> { typeof(Transaction), typeof(Fraud), typeof(Address) };

            for (int i = 0; i < numberOfObjects; i++)
            {
                var selectedClassType = dummyClasses[random.Next(dummyClasses.Count)];

                var newObj = Activator.CreateInstance(selectedClassType) as DummyBase;

                newObj.Sender = "Sender" + random.Next(1, 50);
                newObj.Receiver = "Receiver" + random.Next(1, 50);
                newObj.Amount = random.Next(1500, 6000);

                int numberOfTags = random.Next(1, 4);
                var tagTypes = Enum.GetValues(typeof(ExampleTags)).Cast<ExampleTags>().ToArray();

                for (int j = 0; j < numberOfTags; j++)
                {
                    var randomTagName = tagTypes[random.Next(tagTypes.Length)].ToString();

                    BaseTag newTag = _tagFactory.CreateBaseTag(randomTagName, null, "");
                    await _taggingService.SetTagAsync(newObj, newTag);

                    int numberOfChildTags = random.Next(1, 5);

                    for (int k = 0; k < numberOfChildTags; k++)
                    {
                        var randomChildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                        var childTag = _tagFactory.CreateBaseTag(randomChildTagName, null, $"Child tag {k + 1}");

                        // Set properties for child tag
                        childTag.Parents.Add(newTag.Id);
                        childTag.Value = $"Child Value {k + 1}";

                        // Recursively create child tags for the child tag itself
                        int numberOfGrandchildTags = random.Next(1, 3);

                        for (int m = 0; m < numberOfGrandchildTags; m++)
                        {
                            var randomGrandchildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                            var grandchildTag = _tagFactory.CreateBaseTag(randomGrandchildTagName, null, $"Grandchild tag {m + 1}");

                            // Set properties for grandchild tag
                            grandchildTag.Parents.Add(childTag.Id);
                            grandchildTag.Value = $"Grandchild Value {m + 1}";

                            childTag.AddChildTag(grandchildTag);
                        }

                        newTag.AddChildTag(childTag);
                    }

                    var tags = await _taggingService.GetAllTags(newObj);
                    testData.Add(tags.Select(tag => new KeyValuePair<string, object>(tag.Name, tag)).ToList());
                }
            }

            return testData;
        }
    }
}
