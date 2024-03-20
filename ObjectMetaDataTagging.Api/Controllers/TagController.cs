using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Api.Interfaces;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;
using ObjectMetaDataTagging.Utilities;


namespace ObjectMetaDataTagging.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITaggingManager<BaseTag> _taggingManager;
        private readonly IGenerateTestData _generateTestData;
        private List<IEnumerable<KeyValuePair<string, object>>> testData;
        private bool _processingTag = false;

        public TagController(
            ITaggingManager<BaseTag> taggingManager,
            IGenerateTestData generateTestData
        )
        {
            _taggingManager = taggingManager;
            _generateTestData = generateTestData;
 
            _= InitializeTestData();

        }
        private async Task InitializeTestData()
        {
            try
            {
                _taggingManager.TagAdded += HandleTagAdded;
                testData = await _generateTestData.GenerateTestData();               
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void HandleTagAdded(object sender, AsyncTagAddedEventArgs<BaseTag> e)
        {
            if (_processingTag)
            {
                return;
            }

            try
            {
                _processingTag = true;

                var lastAddedTag = e.Tag;
                var lastAddedObject = _taggingManager.GetObjectByTag(lastAddedTag.Id) as ExamplePersonTransaction;

                if (lastAddedObject != null && lastAddedObject.Amount > 2000)
                {
                    var suspiciousTransferTag = _taggingManager.CreateBaseTag("SuspiciousTransfer (automatically triggered)", null, "Suspicious Transfer Detected");
                    await _taggingManager.SetTagAsync(lastAddedObject, suspiciousTransferTag);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                _processingTag = false;
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

        [HttpGet("filter-items")]
        public async Task<IActionResult> FilterItems()
        {
            try
            {
                if (testData == null)
                {
                    return BadRequest("Test data not initialized");
                }

                Func<BaseTag, bool> propertyFilter = tag =>
                {
                    try
                    {
                        var obj = _taggingManager.GetObjectByTag(tag.Id);
                        if (obj is ExamplePersonTransaction transaction && transaction.Amount < 4000)
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error retrieving object: {ex.Message}");
                    }

                    return false;
                };

                var filteredTags =
                    await _taggingManager
                    .BuildQuery(testData.SelectMany(item => item.Where(kvp => kvp.Value is BaseTag).Select(kvp => (BaseTag)kvp.Value)).ToList(), propertyFilter);


                return Ok(filteredTags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


    }

    internal class Tag : BaseTag
    {
        public Tag()
        {
        }

        public string SomeField { get; set; }
        public string AnotherField { get; set; }
    }
}
