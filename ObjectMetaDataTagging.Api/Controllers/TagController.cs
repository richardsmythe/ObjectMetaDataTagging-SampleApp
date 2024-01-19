using Microsoft.AspNetCore.Mvc;
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
        private IDefaultTaggingService<BaseTag> _taggingService;
        private readonly ITagFactory _tagFactory;
        private readonly IAlertService _alertService;
        private readonly TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> _eventManager;
        private List<IEnumerable<KeyValuePair<string, object>>> testData;

        private static bool isTestDataInitialised = false;

        public TagController(
            IDefaultTaggingService<BaseTag> taggingService,
            ITagFactory tagFactory,
            IAlertService alertService,
            TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> eventManager)
        {
            _taggingService = taggingService;
            _tagFactory = tagFactory;
            _alertService = alertService;
            _eventManager = eventManager;

            // Check if data is already initialised before calling InitialiseTestData
            if (!isTestDataInitialised)
            {
                InitialiseTestData();
                isTestDataInitialised = true;
            }
        }

        private async Task InitialiseTestData()
        {
            _taggingService = new CustomTaggingService<BaseTag>(_eventManager);
            var defaultTaggingService = new DefaultTaggingService<BaseTag>(_taggingService);

            testData = await GenerateTestData(defaultTaggingService, _tagFactory, _alertService);


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
        private static async Task<List<IEnumerable<KeyValuePair<string, object>>>> GenerateTestData(
            IDefaultTaggingService<BaseTag> taggingService,
            ITagFactory tagFactory,
            IAlertService alertService)
        {
            var testData = new List<IEnumerable<KeyValuePair<string, object>>>();
            var random = new Random();
            int numberOfObjects = random.Next(1, 5);

            var dummyClasses = new List<Type> { typeof(Transaction), typeof(Fraud), typeof(Address) };

            for (int i = 0; i < numberOfObjects; i++)
            {
                var selectedClassType = dummyClasses[random.Next(dummyClasses.Count)];
             
                var newObj = Activator.CreateInstance(selectedClassType) as DummyBase;

                newObj.Sender = "Sender" + random.Next(1, 50);
                newObj.Receiver = "Receiver" + random.Next(1, 50);
                newObj.Amount = random.Next(1500, 6000);

                int numberOfTags = random.Next(1, 4);
                var tagTypes = Enum.GetValues(typeof(ExampleTags)).Cast<ExampleTags>().ToArray();

                for (int j = 0; j < numberOfTags; j++)
                {
                    var randomTagName = tagTypes[random.Next(tagTypes.Length)].ToString();

                    BaseTag newTag = tagFactory.CreateBaseTag(randomTagName, null, "");
                    await taggingService.SetTagAsync(newObj, newTag);

                    int numberOfChildTags = random.Next(1, 5);

                    for (int k = 0; k < numberOfChildTags; k++)
                    {
                        var randomChildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                        var childTag = tagFactory.CreateBaseTag(randomChildTagName, null, $"Child tag {k + 1}");

                        // Set properties for child tag
                        childTag.Parents.Add(newTag.Id);
                        childTag.Value = $"Child Value {k + 1}";

                        // Recursively create child tags for the child tag itself
                        int numberOfGrandchildTags = random.Next(1, 3);

                        for (int m = 0; m < numberOfGrandchildTags; m++)
                        {
                            var randomGrandchildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                            var grandchildTag = tagFactory.CreateBaseTag(randomGrandchildTagName, null, $"Grandchild tag {m + 1}");

                            // Set properties for grandchild tag
                            grandchildTag.Parents.Add(childTag.Id);
                            grandchildTag.Value = $"Grandchild Value {m + 1}";  

                            childTag.AddChildTag(grandchildTag);
                        }
                       
                        newTag.AddChildTag(childTag);
                    }
                    
                    var tags = await taggingService.GetAllTags(newObj);
                    testData.Add(tags.Select(tag => new KeyValuePair<string, object>(tag.Name, tag)).ToList());
                }
            }

            return testData;
        }


        [HttpGet("print-object-graph")]
        public async Task<IActionResult> PrintObjectGraph()
        {
            var objectGraph = await _taggingService.GetObjectGraph();
            ObjectGraphBuilder.PrintObjectGraph(objectGraph);
            return Ok(objectGraph);
        }

    }
}
