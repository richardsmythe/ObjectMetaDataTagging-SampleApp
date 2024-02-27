using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Api.Interfaces;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Utilities;
using System.Text.Json.Serialization;
using System.Text.Json;


namespace ObjectMetaDataTagging.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITaggingManager<BaseTag> _taggingManager;
        private readonly IGenerateTestData _generateTestData;
        private List<IEnumerable<KeyValuePair<string, object>>> testData;

        public TagController(
            ITaggingManager<BaseTag> taggingManager,
            IGenerateTestData generateTestData
        )
        {
            _taggingManager = taggingManager;
            _generateTestData = generateTestData;

            InitializeTestData();
        }


        private async Task InitializeTestData()
        {
            try
            {
                testData = await _generateTestData.GenerateTestData();
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("generateTags")]
        public async Task<ActionResult<List<IEnumerable<KeyValuePair<string, object>>>>> GenerateTags()
        {
            if (testData == null)
            {
                return BadRequest("Test data not initialized");
            }

            return Ok(testData);
        }

        [HttpGet("map-tag")]
        public async Task<IActionResult> MapTag()
        {
            try
            {
                var tagToMap = testData
                    .SelectMany(item => item
                    .Where(kvp => kvp.Value is BaseTag)
                    .Select(kvp => (BaseTag)kvp.Value))
                    .FirstOrDefault();

                if (tagToMap == null) return Ok("No tags available to map.");

                Tag tag = new Tag()
                {
                    SomeField = "Test",
                    AnotherField = "Test",
                };

                BaseTag mappedTag = await _taggingManager.MapTagsBetweenTypes(tagToMap, tag);

                return Ok(mappedTag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("print-object-graph")]
        public async Task<IActionResult> PrintObjectGraph()
        {
            var objectGraph = await _taggingManager.GetObjectGraph();
            ObjectGraphBuilder.PrintObjectGraph(objectGraph);
            return Ok(objectGraph);
        }
    }

    internal class Tag:BaseTag
    {
        public Tag()
        {
        }

        public string SomeField { get; set; }
        public string AnotherField { get; set; }
    }
}
