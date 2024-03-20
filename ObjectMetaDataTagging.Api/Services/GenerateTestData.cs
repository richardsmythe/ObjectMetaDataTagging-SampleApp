using ObjectMetaDataTagging.Api.Interfaces;
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

        // Generates 3 levels of tags on objects example purposes.
        async Task<List<IEnumerable<KeyValuePair<string, object>>>> IGenerateTestData.GenerateTestData()
        {
            var testData = new List<IEnumerable<KeyValuePair<string, object>>>();
            var random = new Random();
            int numberOfObjects = random.Next(3, 5);
            var dummyClasses = new List<Type> { typeof(UserTransaction), typeof(Fraud), typeof(Address), typeof(Beneficiary) };

            for (int i = 0; i < numberOfObjects; i++)
            {
                var selectedClassType = dummyClasses[random.Next(dummyClasses.Count)];
                var newObj = Activator.CreateInstance(selectedClassType) as ExamplePersonTransaction;

                newObj.Sender = "Sender" + random.Next(1, 50);
                newObj.Receiver = "Receiver" + random.Next(1, 50);
                newObj.Amount = random.Next(1000, 7000);

                int numberOfTags = random.Next(1, 3);
                var tagTypes = Enum.GetValues(typeof(ExampleTags)).Cast<ExampleTags>().ToArray();

                for (int j = 0; j < numberOfTags; j++)
                {
                    var randomTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                    BaseTag newTag = _tagFactory.CreateBaseTag(randomTagName, null, "");
                    await _taggingManager.SetTagAsync(newObj, newTag);

                    int numberOfChildTags = random.Next(1, 2);

                    for (int k = 0; k < numberOfChildTags; k++)
                    {
                        var randomChildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                        var childTag = _tagFactory.CreateBaseTag(randomChildTagName, null, $"Child tag {k + 1}");

                        childTag.Parents.Add(newTag.Id);
                        childTag.Value = $"Child Value {k + 1}";

                        int numberOfGrandchildTags = random.Next(1,3);

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

        public class Fraud : ExamplePersonTransaction
        {
        }

        public class Address : ExamplePersonTransaction
        {    
        }

        public class Beneficiary : ExamplePersonTransaction
        {

        }
        public class UserTransaction : ExamplePersonTransaction
        {

        }

    }
}