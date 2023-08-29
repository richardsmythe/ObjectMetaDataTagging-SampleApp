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
            var objectName = "";
            foreach (var obj in testData)
            {
                // gets type --> obj.First().Key.GetType().Name.ToString(); 
                if (obj.First().Value is BaseTag baseTag)
                {
                    objectName = baseTag.AssociatedParentObjectName.ToString();
                }
                var objectId = Guid.NewGuid();
                var tags = obj.Select(kv => kv.Value.ToString());//?.Split(',')[1].TrimEnd(']')).ToList();

                objectModels.Add(new ObjectModel
                {
                    Id = objectId,
                    ObjectName = objectName
                });

                tagModels.AddRange(obj.Select(tagPair =>
                {
                    var tag = tagPair.Value as BaseTag;
                    if (tag != null)
                    {
                        return new TagModel
                        {
                            TagName = tag.Name,
                            Description = tag.Description,
                            AssociatedObject = objectName,
                            AssociatedObjectId = objectId,
                        };
                    }
                    return null;
                }).Where(tagModel => tagModel != null)!);
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

            var alertService = new AlertService(null!); 
            var tagAddedHandler = new TagAddedHandler(alertService);
            var tagRemovedHandler = new TagRemovedHandler();
            var tagUpdatedHandler = new TagUpdatedHandler();

            var eventManager = new TaggingEventManager<TagAddedEventArgs,
                                                        TagRemovedEventArgs,
                                                        TagUpdatedEventArgs>(tagAddedHandler, tagRemovedHandler, tagUpdatedHandler);

            var taggingService = new DefaultTaggingService(eventManager);

            // Assign the actual taggingService to the alertService:
            var fieldInfo = alertService.GetType().GetField("_taggingService", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(alertService, taggingService);
            }

            // Create example parent object that will trigger event
            var trans1 = new ExamplePersonTransaction { Sender = "John", Receiver = "Richard", Amount = 4545 };

            // Create new tag child object
            var fundTransferTag = new BaseTag("Transfering Funds", ExampleTags.FundsTransfer);
            taggingService.SetTag(trans1, fundTransferTag);


            testData.Add(taggingService.GetAllTags(trans1)
                .Select(tag => new KeyValuePair<string, object>(tag.Name, tag)).ToList());

            return testData;
        }

    }
}
