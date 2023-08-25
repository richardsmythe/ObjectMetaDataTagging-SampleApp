using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Api.Models;
using System.Reflection;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;

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

            var addedHandler = new TestAddedHandler();
            var removedHandler = new TestRemovedHandler();  
            var updatedHandler = new TestUpdatedHandler();  
            var eventManager = new TaggingEventManager<TagAddedEventArgs,
                                                        TagRemovedEventArgs,
                                                        TagUpdatedEventArgs>(addedHandler, removedHandler, updatedHandler);

            var taggingService = new DefaultTaggingService(eventManager);
            var alertService = new AlertService(taggingService);
        

            var trans1 = new ExamplePersonTransaction { Sender = "John", Receiver = "Richard", Amount = 3333 };

            taggingService.TagAdded += (sender, args) =>
            {
                alertService.IsSuspiciousTransaction(trans1);
            };

            var fundTransferTag = new BaseTag("Transfering Funds", ExampleTags.FundsTransfer);

            taggingService.SetTag(trans1,fundTransferTag);
            
            testData.Add(taggingService.GetAllTags(trans1).ToList());

            return testData;
        }
        public class TestAddedHandler : IEventHandler<TagAddedEventArgs>
        {
            public void Handle(TagAddedEventArgs args)
            {
                // Logic you want to perform when a tag is added.
            }
        }

        public class TestRemovedHandler : IEventHandler<TagRemovedEventArgs>
        {
            public void Handle(TagRemovedEventArgs args)
            {
                // You can leave this empty if you're not interested in this event.
            }
        }

        public class TestUpdatedHandler : IEventHandler<TagUpdatedEventArgs>
        {
            public void Handle(TagUpdatedEventArgs args)
            {
                // Again, you can leave this empty if you're not interested.
            }
        }

    }
}
