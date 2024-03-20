using Microsoft.AspNetCore.Mvc.ApplicationParts;
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
            int numberOfObjects = random.Next(3, 4);
            var dummyClasses = new List<Type> { typeof(UserTransaction), typeof(Fraud), typeof(Address), typeof(Beneficiary) };
            // Create a list to store references to newObj instances
            List<ExamplePersonTransaction> newObjectsList = new List<ExamplePersonTransaction>();
            
            for (int i = 0; i < numberOfObjects; i++)
            {
                // Choose a random class type from the list of dummy classes
                var selectedClassType = dummyClasses[random.Next(dummyClasses.Count)];

                // Create an instance of the selected class type and cast it to ExamplePersonTransaction
                var newObj = Activator.CreateInstance(selectedClassType) as ExamplePersonTransaction;

                // Add newObj to the list
                newObjectsList.Add(newObj);

                // Generate random sender, receiver, and amount for the object
                newObj.Sender = "Sender" + random.Next(1, 50);
                newObj.Receiver = "Receiver" + random.Next(1, 50);
                newObj.Amount = random.Next(1000, 7000);

                // Generate a random number of tags for the object
                int numberOfTags = random.Next(1, 3);
                var tagTypes = Enum.GetValues(typeof(ExampleTags)).Cast<ExampleTags>().ToArray();

                for (int j = 0; j < numberOfTags; j++)
                {
                    // Choose a random tag name from the list of tag types
                    var randomTagName = tagTypes[random.Next(tagTypes.Length)].ToString();

                    // Create a new base tag with the random tag name
                    BaseTag newTag = _tagFactory.CreateBaseTag(randomTagName, null, "");

                    // Associate the tag with the current object
                    await _taggingManager.SetTagAsync(newObj, newTag);

                    // Generate a random number of child tags for the current tag
                    int numberOfChildTags = random.Next(1, 2);

                    for (int k = 0; k < numberOfChildTags; k++)
                    {
                        // Choose a random child tag name from the list of tag types
                        var randomChildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();

                        // Create a new base tag for the child tag
                        var childTag = _tagFactory.CreateBaseTag(randomChildTagName, null, $"Child tag {k + 1}");

                        // Add the parent tag's ID as a parent of the child tag
                        childTag.Parents.Add(newTag.Id);

                        // Set value for the child tag
                        childTag.Value = $"Child Value {k + 1}";

                        // Generate a random number of grandchild tags for the child tag
                        int numberOfGrandchildTags = random.Next(1, 3);

                        for (int m = 0; m < numberOfGrandchildTags; m++)
                        {
                            // Choose a random grandchild tag name from the list of tag types
                            var randomGrandchildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();

                            // Create a new base tag for the grandchild tag
                            var grandchildTag = _tagFactory.CreateBaseTag(randomGrandchildTagName, null, $"Grandchild tag {m + 1}");

                            // Add the child tag's ID as a parent of the grandchild tag
                            grandchildTag.Parents.Add(childTag.Id);

                            // Set value for the grandchild tag
                            grandchildTag.Value = $"Grandchild Value {m + 1}";

                            // Add the grandchild tag to the child tag
                            childTag.AddChildTag(grandchildTag);
                        }

                        // Add the child tag to the parent tag
                        newTag.AddChildTag(childTag);
                    }

                    // Retrieve all tags associated with the current object
                    var tags = await _taggingManager.GetAllTags(newObj);

                    // Add the tags to the test data list
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