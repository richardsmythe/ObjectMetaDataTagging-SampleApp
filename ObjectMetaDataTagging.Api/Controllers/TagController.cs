using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Api.Interfaces;
using ObjectMetaDataTagging.Api.Services;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;
using ObjectMetaDataTagging.Utilities;

namespace ObjectMetaDataTagging.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private ObjectMetaDataTaggingFacade<BaseTag> _taggingFacade;

        private IDefaultTaggingService<BaseTag> _taggingService;
        private readonly ITagFactory _tagFactory;
        private readonly IAlertService _alertService;
        private readonly IGenerateTestData _generateTestData;
        private readonly TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> _eventManager;
        private List<IEnumerable<KeyValuePair<string, object>>> testData;

        private static bool isTestDataInitialised = false;

        public TagController(
             ObjectMetaDataTaggingFacade<BaseTag> taggingFacade,
            IDefaultTaggingService<BaseTag> taggingService,
            ITagFactory tagFactory,
            IAlertService alertService,
            IGenerateTestData generateTestData,
            TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> eventManager)
        {
            _taggingFacade = taggingFacade ?? throw new ArgumentNullException(nameof(taggingFacade));

            _taggingService = taggingService;
            _tagFactory = tagFactory;
            _alertService = alertService;
            _eventManager = eventManager;
            _generateTestData = generateTestData;

            // Check if data is already initialised before calling InitialiseTestData
            if (!isTestDataInitialised)
            {
                InitialiseTestData();
                isTestDataInitialised = true;
            }
        }

        private async Task InitialiseTestData()
        {
            testData = await _generateTestData.GenerateTestData();
        }

        //[HttpDelete]
        //public async Task<IActionResult> DeleteAsync(Guid tagId)
        //{
        //    var obj = _taggingService.GetObjectByTag(tagId);
        //    if (obj != null && await _taggingService.RemoveTagAsync(obj, tagId))
        //    {
        //        var updatedTags = _taggingService.GetAllTags(obj);
        //        var objectModels = new List<ObjectModel>();
        //        var tagModels = new List<TagModel>();
        //        var objectName = "";
        //        Guid objectId = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF00"); ;
        //        foreach (var updatedTag in await updatedTags)
        //        {
        //            var tagModel = new TagModel
        //            {
        //                tagId = updatedTag.Id,
        //                TagName = updatedTag.Name,
        //                Description = updatedTag.Description,
        //                AssociatedObject = updatedTag.AssociatedParentObjectName?.ToString(),
        //                AssociatedObjectId = updatedTag.AssociatedParentObjectId,
        //            };
        //            tagModels.Add(tagModel);

        //            if (updatedTag.AssociatedParentObjectName != null)
        //            {
        //                objectName = updatedTag.AssociatedParentObjectName?.ToString();
        //                objectId = updatedTag.AssociatedParentObjectId;
        //            }
        //        }

        //        objectModels.Add(new ObjectModel
        //        {
        //            Id = objectId,
        //            ObjectName = objectName
        //        });

        //        var frameModel = new Frame
        //        {
        //            Id = 0,
        //            Origin = Assembly.GetEntryAssembly().GetName().Name,
        //            ObjectData = objectModels,
        //            TagData = tagModels
        //        };

        //        return Ok(new List<Frame> { frameModel });
        //    }

        //    return NotFound();
        //}

        // Initial data for app
        //[HttpGet]
        //public IActionResult GetObjectsAndTags()
        //{
        //    InitialiseTestData();
        //    var objectModels = new List<ObjectModel>();
        //    var tagModels = new List<TagModel>();
        //    var objectName = "";
        //    Guid objectId = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF00");

        //    foreach (var obj in testData)
        //    {
        //        //if (obj.First().Value is BaseTag baseTag && baseTag != null)
        //        //{
        //        //    objectName = baseTag.AssociatedParentObjectName?.ToString() ?? "";
        //        //    objectId = baseTag.AssociatedParentObjectId;
        //        //}

        //        var tags = obj.Select(kv => kv.Value.ToString());

        //        objectModels.Add(new ObjectModel
        //        {
        //            Id = objectId,
        //            ObjectName = objectName
        //        });

        //        tagModels.AddRange(obj.Select(tagPair =>
        //        {
        //            var tag = tagPair.Value as BaseTag;
        //            if (tag != null)
        //            {
        //                var tagModel = CreateTagModel(tag, objectName, objectId);
        //                return tagModel;
        //            }

        //            return null;
        //        }).Where(tagModel => tagModel != null)!);
        //    }

        //    var frameModel = new Frame
        //    {
        //        Id = 1,
        //        Origin = Assembly.GetEntryAssembly().GetName().Name,
        //        ObjectData = objectModels,
        //        TagData = tagModels
        //    };

        //    return Ok(new List<Frame> { frameModel });
        //    }

        //    private static TagModel CreateTagModel(BaseTag tag, string objectName, Guid objectId)
        //{
        //    var tagModel = new TagModel
        //    {
        //        TagId = tag.Id,
        //        TagName = tag.Name,
        //        Description = tag.Description,
        //        AssociatedObject = objectName,
        //        AssociatedObjectId = objectId,
        //        ChildTags = tag.ChildTags?.Select(childTag => CreateTagModel(childTag, tag.Name, tag.Id)).ToList()
        //    };

        //    // Set properties for child tags
        //    if (tagModel.ChildTags != null)
        //    {
        //        foreach (var childTag in tagModel.ChildTags)
        //        {
        //            childTag.AssociatedObject = tagModel.TagName;
        //            childTag.AssociatedObjectId = tagModel.TagId;
        //        }
        //    }
        //    return tagModel;
        //}

        // generate dummy data with possibility of 3 generations of child tag


        [HttpGet("print-object-graph")]
        public async Task<IActionResult> PrintObjectGraph()
        {
            var objectGraph = await _taggingFacade.GetObjectGraph();
            ObjectGraphBuilder.PrintObjectGraph(objectGraph);
            return Ok(objectGraph);
        }

    }
}
