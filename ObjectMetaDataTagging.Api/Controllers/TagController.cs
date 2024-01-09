using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Api.Models;
using System.Reflection;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Services;
using ObjectMetaDataTagging.Api.Services;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private IDefaultTaggingService<BaseTag> _taggingService;
        private readonly IDynamicQueryBuilder<BaseTag, DefaultFilterCriteria> _dynamicQueryBuilder;
        private readonly ITagFactory _tagFactory;
        private readonly IAlertService _alertService;
        private readonly TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> _eventManager;
        private List<IEnumerable<KeyValuePair<string, object>>> testData;

        private static bool isTestDataInitialised = false;

        public TagController(
            IDynamicQueryBuilder<BaseTag, DefaultFilterCriteria> dynamicQueryBuilder,
            IDefaultTaggingService<BaseTag> taggingService,
            ITagFactory tagFactory,
            IAlertService alertService,
            TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> eventManager)
        {
            _taggingService = taggingService ?? throw new ArgumentNullException(nameof(taggingService));
            _tagFactory = tagFactory ?? throw new ArgumentNullException(nameof(tagFactory));
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            _eventManager = eventManager;
            _dynamicQueryBuilder = dynamicQueryBuilder;

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

            testData = await GenerateTestData(defaultTaggingService, _tagFactory, _alertService, _dynamicQueryBuilder);


        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(Guid tagId)
        {
            var obj = _taggingService.GetObjectByTag(tagId);
            if (obj != null && await _taggingService.RemoveTagAsync(obj, tagId))
            {
                var updatedTags = _taggingService.GetAllTags(obj);
                var objectModels = new List<ObjectModel>();
                var tagModels = new List<TagModel>();
                var objectName = "";
                Guid objectId = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF00"); ;
                foreach (var updatedTag in await updatedTags)
                {
                    var tagModel = new TagModel
                    {
                        tagId = updatedTag.Id,
                        TagName = updatedTag.Name,
                        Description = updatedTag.Description,
                        AssociatedObject = updatedTag.AssociatedParentObjectName?.ToString(),
                        AssociatedObjectId = updatedTag.AssociatedParentObjectId,
                    };
                    tagModels.Add(tagModel);

                    if (updatedTag.AssociatedParentObjectName != null)
                    {
                        objectName = updatedTag.AssociatedParentObjectName?.ToString();
                        objectId = updatedTag.AssociatedParentObjectId;
                    }
                }

                objectModels.Add(new ObjectModel
                {
                    Id = objectId,
                    ObjectName = objectName
                });

                var frameModel = new Frame
                {
                    Id = 0,
                    Origin = Assembly.GetEntryAssembly().GetName().Name,
                    ObjectData = objectModels,
                    TagData = tagModels
                };

                return Ok(new List<Frame> { frameModel });
            }

            return NotFound();
        }

        // Initial data for app
        [HttpGet]
        public IActionResult GetObjectsAndTags()
        {
            var objectModels = new List<ObjectModel>();
            var tagModels = new List<TagModel>();
            var objectName = "";
            Guid objectId = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF00");

            foreach (var obj in testData)
            {
                if (obj.First().Value is BaseTag baseTag && baseTag != null)
                {
                    objectName = baseTag.AssociatedParentObjectName?.ToString() ?? "";
                    objectId = baseTag.AssociatedParentObjectId;
                }

                var tags = obj.Select(kv => kv.Value.ToString());

                objectModels.Add(new ObjectModel
                {
                    Id = objectId,
                    ObjectName = objectName
                });

                tagModels.AddRange(obj.Select(tagPair =>
                {
                    var tag = tagPair.Value as BaseTag;
                    if (tag != null)
                    {
                        var tagModel = CreateTagModel(tag, objectName, objectId);
                        return tagModel;
                    }

                    return null;
                }).Where(tagModel => tagModel != null)!);
            }

            var frameModel = new Frame
            {
                Id = 1,
                Origin = Assembly.GetEntryAssembly().GetName().Name,
                ObjectData = objectModels,
                TagData = tagModels
            };

            return Ok(new List<Frame> { frameModel });
        }

        private static TagModel CreateTagModel(BaseTag tag, string objectName, Guid objectId)
        {
            var tagModel = new TagModel
            {
                tagId = tag.Id,
                TagName = tag.Name,
                Description = tag.Description,
                AssociatedObject = objectName,
                AssociatedObjectId = objectId,
                ChildTags = tag.ChildTags?.Select(childTag => CreateTagModel(childTag, tag.Name, tag.Id)).ToList()
            };

            // Set properties for child tags
            if (tagModel.ChildTags != null)
            {
                foreach (var childTag in tagModel.ChildTags)
                {
                    childTag.AssociatedObject = tagModel.TagName;
                    childTag.AssociatedObjectId = tagModel.tagId;
                }
            }

            return tagModel;
        }


        private static async Task<List<IEnumerable<KeyValuePair<string, object>>>> GenerateTestData(
            IDefaultTaggingService<BaseTag> taggingService,
            ITagFactory tagFactory,
            IAlertService alertService,
            IDynamicQueryBuilder<BaseTag, DefaultFilterCriteria> queryBuilder)
        {
            var testData = new List<IEnumerable<KeyValuePair<string, object>>>();
            var random = new Random();
            int numberOfObjects = random.Next(1, 5);

            for (int i = 0; i < numberOfObjects; i++)
            {
                var newObj = new ExamplePersonTransaction
                {
                    Sender = "Sender" + random.Next(1, 50),
                    Receiver = "Receiver" + random.Next(1, 50),
                    Amount = random.Next(1, 5999),
                };

                int numberOfTags = random.Next(1, 10);
                var tagTypes = Enum.GetValues(typeof(ExampleTags)).Cast<ExampleTags>().ToArray();

                for (int j = 0; j < numberOfTags; j++)
                {
                    var tagName = tagTypes[random.Next(tagTypes.Length)].ToString();

                    BaseTag newTag = tagFactory.CreateBaseTag(tagName, null, "");
                    await taggingService.SetTagAsync(newObj, newTag);

                    int numberOfChildTags = random.Next(0, 5);

                    for (int k = 0; k < numberOfChildTags; k++)
                    {
                        var childTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                        var childTag = tagFactory.CreateBaseTag(childTagName, null, $"Child tag {k + 1}");

                        // Set properties for child tag
                        childTag.AssociatedParentObjectName = newTag.Name;
                        childTag.AssociatedParentObjectId = newTag.Id;
                        childTag.Value = $"Child Value {k + 1}";

                        // Recursively create child tags for the child tag itself
                        int numberOfGrandchildTags = random.Next(0, 3);

                        for (int m = 0; m < numberOfGrandchildTags; m++)
                        {
                            var grandchildTagName = tagTypes[random.Next(tagTypes.Length)].ToString();
                            //var grandchildTagName = "Grandchild tag";
                            var grandchildTag = tagFactory.CreateBaseTag(grandchildTagName, null, $"Grandchild tag {m + 1}");

                            // Set properties for grandchild tag
                            grandchildTag.AssociatedParentObjectName = childTag.Name;
                            grandchildTag.AssociatedParentObjectId = childTag.Id;
                            grandchildTag.Value = $"Grandchild Value {m + 1}";

                            childTag.AddChildTag(grandchildTag);
                        }

                        newTag.AddChildTag(childTag);
                    }

                    var tags = await taggingService.GetAllTags(newObj);
                    testData.Add(tags.Select(tag => new KeyValuePair<string, object>(tag.Name, tag)).ToList());
                }
            }


            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            //var trans1 = new ExamplePersonTransaction { Sender = "John", Receiver = "Richard", Amount = 54123 };

            //var fundTransferTag = tagFactory.CreateBaseTag("Transfering Funds", ExampleTags.FundsTransfer, null);
            //taggingService.SetTagAsync(trans1, fundTransferTag);

            //var fundTransferTag2 = tagFactory.CreateBaseTag("Payment Expired", ExampleTags.PaymentExpired, null);
            //taggingService.SetTagAsync(trans1, fundTransferTag2);

            //testData.Add(taggingService.GetAllTags(trans1)
            //    .Select(tag => new KeyValuePair<string, object>(tag.Name, tag)).ToList());

            //var trans2 = new ExamplePersonTransaction { Sender = "Ed", Receiver = "Tim", Amount = 123 };

            //var fundTransferTag3 = tagFactory.CreateBaseTag("Transfering Funds", ExampleTags.AccountActivity, "A transcation occured");
            //taggingService.SetTagAsync(trans2, fundTransferTag3);

            //var fundTransferTag4 = tagFactory.CreateBaseTag("Transfering Funds", ExampleTags.AccountActivity, "A transcation occured");
            //taggingService.SetTagAsync(trans2, fundTransferTag4);

            //testData.Add(taggingService.GetAllTags(trans2)
            //  .Select(tag => new KeyValuePair<string, object>(tag.Name, tag)).ToList());

            /////////// Dynamic Filter Test /////////////

            //var customFilter = new CustomFilter("Suspicious Transfer", "ExampleTags");

            //var filteredRequest = queryBuilder.BuildDynamicQuery<BaseTag>(
            //    trans1.AssociatedTags,
            //    tag => tag.Name == customFilter.Name, // these define the filter condition for delegate based filtering
            //    tag => tag.Type == customFilter.Type,
            //    LogicalOperator.AND
            //);

            //////////////////////////////////////////////

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
