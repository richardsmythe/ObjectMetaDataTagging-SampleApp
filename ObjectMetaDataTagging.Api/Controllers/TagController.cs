using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Api.Interfaces;
using ObjectMetaDataTagging.Api.Models;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            _ = InitialiseTestData();
        }

        private async Task InitialiseTestData()
        {
            // Generates the dummy data on which this web app
            // uses to demonstrate the usage of the tagging lirbary.
            testData = await _generateTestData.GenerateTestData();
        }

        [HttpGet("filter-tags")]
        public async Task<IActionResult> FilterTags()
        {
            try
            {
                var tags = testData
                   .SelectMany(item => item
                   .Where(kvp => kvp.Value is BaseTag)
                   .Select(kvp => (BaseTag)kvp.Value))
                   .ToList();
                
                Func<BaseTag, bool> myFilter = tag => {
                    return tag.Name == ExampleTags.NameDuplication.ToString() 
                    ;
                };
                var filteredTags = await _taggingManager
                    .BuildQuery(tags, myFilter, LogicalOperator.AND);

                if (filteredTags.Any())
                {
                    var options = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve,
                        WriteIndented = true,
                    };

                    var jsonString = JsonSerializer.Serialize(filteredTags, options);

                    return Ok(jsonString);
                }

                return Ok("No tags meet the filter criteria.");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("map-tag")]
        public async Task<IActionResult> MapTag()
        {
            try
            {
                var sourceTagToMap = testData
                    .SelectMany(item => item
                    .Where(kvp => kvp.Value is BaseTag)
                    .Select(kvp => (BaseTag)kvp.Value))
                    .FirstOrDefault();

                if (sourceTagToMap == null) return Ok("No tags available to map.");

                Tag targetTag = new Tag()
                {
                    SomeField = "Test",
                    AnotherField = "Test",
                };

                var mappedTag = await _taggingManager.MapTagsBetweenTypes(sourceTagToMap, targetTag);

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                    WriteIndented = true,
                };

                var jsonString = JsonSerializer.Serialize(mappedTag, options);

                return Ok(jsonString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        [HttpGet("print-object-graph")]
        public async Task<IActionResult> PrintObjectGraph()
        {
            try
            {
                var objectGraph = await _taggingManager.GetObjectGraph();
                ObjectGraphBuilder.PrintObjectGraph(objectGraph);

                return Ok(objectGraph);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occured: {ex.Message}");
            }
        }
    }
}
