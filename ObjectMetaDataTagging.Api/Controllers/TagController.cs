using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Api.Models;
using System.Reflection;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Api.Events;

namespace ObjectMetaDataTagging.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {

        public TagController()
        {

        }

        [HttpGet]
        public IActionResult GetObjectsAndTags()
        {
            var testData = GenerateTestData();

            var objectModels = new List<ObjectModel>();
            var tagModels = new List<TagModel>();

            foreach (var obj in testData)
            {
                // gets type --> obj.First().Key.GetType().Name.ToString(); 
                var objectName = obj.First().Key.ToString().Split('.').Last();
                var objectId = Guid.NewGuid();
                var tags = obj.Select(kv => kv.Value.ToString()?.Split(',')[1].TrimEnd(']')).ToList();

                objectModels.Add(new ObjectModel
                {
                    Id = objectId,
                    ObjectName = objectName
                });

                tagModels.AddRange(tags.Select(tagName => new TagModel
                {
                    TagName = tagName,
                    AssociatedObject = objectName,
                    AssociatedObjectId = objectId
                }));
            }

            var frameModel = new Frame
            {
                Id = Guid.NewGuid(),

                Origin = Assembly.GetEntryAssembly().GetName().Name,
                ObjectData = objectModels,
                TagData = tagModels
            };

            return Ok(new List<Frame> { frameModel });
        }

        public static List<IEnumerable<KeyValuePair<string, object>>> GenerateTestData()
        {
            var testData = new List<IEnumerable<KeyValuePair<string, object>>>();

            // First, create the handlers:
            var alertService = new AlertService(null); // Temporarily pass in null, we will re-assign it later.
            var addedHandler = new TestAddedHandler(alertService);
            var removedHandler = new TestRemovedHandler();
            var updatedHandler = new TestUpdatedHandler();

            var eventManager = new TaggingEventManager<TagAddedEventArgs,
                                                        TagRemovedEventArgs,
                                                        TagUpdatedEventArgs>(addedHandler, removedHandler, updatedHandler);

            var taggingService = new DefaultTaggingService(eventManager);

            // Assign the actual taggingService to the alertService:
            var fieldInfo = alertService.GetType().GetField("_taggingService", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(alertService, taggingService);
            }

            var trans1 = new ExamplePersonTransaction { Sender = "John", Receiver = "Richard", Amount = 3333 };

            var fundTransferTag = new BaseTag("Transfering Funds", ExampleTags.FundsTransfer);
            taggingService.SetTag(trans1, fundTransferTag);

            testData.Add(taggingService.GetAllTags(trans1).ToList());

            return testData;
        }
      

    }
}
