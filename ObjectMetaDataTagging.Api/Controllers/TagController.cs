using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Extensions;
using ObjectMetaDataTagging.Api.Models;
using System.Reflection;
using ObjectMetaDataTagging.Models;

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
                var objectName = obj.First().Key.ToString();
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
            var fundTransferTag = new BaseTag("Transfering Funds", ExampleTags.FundsTransfer);
            var trans1 = new ExamplePersonTransaction { Sender = "John", Receiver = "Richard", Amount = 3333 };
            trans1.SetTag(fundTransferTag);
            testData.Add(trans1.GetAllTags().ToList());          

            return testData;
        }
    }
}
