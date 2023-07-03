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
                var tags = obj.Select(kv => new KeyValuePair<string, string>(objectName, kv.Value.ToString())).ToList();

                objectModels.Add(new ObjectModel
                {
                    Id = GenerateObjectId(),
                    Name = objectName
                });

                tagModels.AddRange(tags.Select(tag => new TagModel
                {
                    //Id = GenerateTagId(),
                    Name = tag.Value,
                    AssociatedObject = tag.Key
                }));
            }

            var frameModel = new Frame
            {
                Id = GenerateFrameId(),
                //Title = "Frame 1",
                ObjectModel = objectModels,
                TagModel = tagModels
            };

            return Ok(new List<Frame> { frameModel });
        }


        private Guid GenerateFrameId()
        {
            return Guid.NewGuid();
        }

        private static int objectIdCounter = 0;
        private static int tagIdCounter = 0;

        private int GenerateObjectId()
        {
            int uniqueId = objectIdCounter++; // Increment the counter to generate a new unique ID
            return uniqueId;
        }

        private int GenerateTagId()
        {
            int uniqueId = tagIdCounter++; // Increment the counter to generate a new unique ID
            return uniqueId;
        }
    }
}