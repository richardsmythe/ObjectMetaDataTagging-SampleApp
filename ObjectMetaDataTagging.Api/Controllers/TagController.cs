using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Extensions;
using ObjectMetaDataTagging.Api.Models;
using System.Reflection;

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
                var tags = obj.Select(kv => kv.Value.ToString().Split(',')[1].TrimEnd(']')).ToList();

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
    }
}