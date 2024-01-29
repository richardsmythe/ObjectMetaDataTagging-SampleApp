using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Api.Interfaces;
using ObjectMetaDataTagging.Api.Services;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;
using ObjectMetaDataTagging.Utilities;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ObjectMetaDataTagging.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {

        private readonly IObjectMetaDataTaggingFacade<BaseTag> _taggingFacade;
        private readonly IGenerateTestData _generateTestData;
        private List<IEnumerable<KeyValuePair<string, object>>> testData;

        public TagController(
            IObjectMetaDataTaggingFacade<BaseTag> taggingFacade,
            IGenerateTestData generateTestData
            )
        {
            _taggingFacade = taggingFacade;
            _generateTestData = generateTestData;
            InitialiseTestData();
        }

        private async Task InitialiseTestData()
        {
            testData = await _generateTestData.GenerateTestData();

        }

        [HttpGet("filter-tags")]
        public async Task<IActionResult> FilterTags()
        {
            try
            {
                Func<BaseTag, bool> myFilter = tag =>
                    tag.Name == ExampleTags.NameDuplication.ToString() //&& tag.ChildTags.Count > 0
                    ;
                var tags = testData
                   .SelectMany(item => item
                   .Where(kvp => kvp.Value is BaseTag)
                   .Select(kvp => (BaseTag)kvp.Value))
                   .ToList();

                var filteredTags = await _taggingFacade
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


        [HttpGet("print-object-graph")]
        public async Task<IActionResult> PrintObjectGraph()
        {
            try
            {
                var objectGraph = await _taggingFacade.GetObjectGraph();
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
