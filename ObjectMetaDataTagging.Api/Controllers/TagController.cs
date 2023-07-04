using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Extensions;
using ObjectMetaDataTagging.Api.Models;

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
            var testData = ObjectTaggingExtensions.GenerateTestData();

            var objectModels = new List<ObjectModel>();
            var tagModels = new List<TagModel>();

            foreach (var obj in testData)
            {
                var objectName = obj.First().Key.ToString();
                var objectId = obj.GetHashCode();
                var tags = obj.Select(kv => new KeyValuePair<string, string>(objectName, kv.Value.ToString())).ToList();

                objectModels.Add(new ObjectModel
                {
                    Id = objectId,
                    Name = objectName
                });

                tagModels.AddRange(tags.Select(tag => new TagModel
                {

                    Name = tag.Value,
                    AssociatedObject = tag.Key,
                    AssociatedObjectId = objectId
                }));
            }

            var frameModel = new Frame
            {
                Id = Guid.NewGuid(),
                FrameName = "New Frame",
                ObjectModel = objectModels,
                TagModel = tagModels
            };

            return Ok(new List<Frame> { frameModel });
        }
    }
}