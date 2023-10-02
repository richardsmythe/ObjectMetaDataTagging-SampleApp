using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Api.Models;
using System.Reflection;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Api.Events;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Models.QueryModels;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace ObjectMetaDataTagging.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly IDynamicQueryBuilder<BaseTag, DefaultFilterCriteria> _dynamicQueryBuilder;
        private readonly IDefaultTaggingService _taggingService;
        private readonly ITagFactory _tagFactory;
        private readonly IAlertService _alertService;
        private readonly TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs> _eventManager;

        public TagController(
            IDynamicQueryBuilder<BaseTag, DefaultFilterCriteria> dynamicQueryBuilder,
            IDefaultTaggingService taggingService,
            ITagFactory tagFactory,
            IAlertService alertService,
            TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs> eventManager)
        {
            _taggingService = taggingService ?? throw new ArgumentNullException(nameof(taggingService));
            _tagFactory = tagFactory ?? throw new ArgumentNullException(nameof(tagFactory));
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            _eventManager = eventManager;
            _dynamicQueryBuilder = dynamicQueryBuilder;
        }


        [HttpDelete]
        public IActionResult Delete(Guid tagId)
        {
            var obj = _taggingService.GetObjectByTag(tagId); 
            if (obj != null && _taggingService.RemoveTag(obj, tagId))
            {
                var updatedTags = _taggingService.GetAllTags(obj);

                var objectModels = new List<ObjectModel>();
                var tagModels = new List<TagModel>();
                var objectName = "";
                Guid objectId = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF00"); ;
                foreach (var updatedTag in updatedTags)
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
                    
                    if(updatedTag.AssociatedParentObjectName != null)
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
            var testData = GenerateTestData(_taggingService, _tagFactory, _alertService, _dynamicQueryBuilder);

            var objectModels = new List<ObjectModel>();
            var tagModels = new List<TagModel>();
            var objectName = "";
            Guid objectId = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF00");
            foreach (var obj in testData)
            {
                // gets type --> obj.First().Key.GetType().Name.ToString(); 
                if (obj.First().Value is BaseTag baseTag)
                {
                    objectName = baseTag.AssociatedParentObjectName.ToString();
                    objectId = baseTag.AssociatedParentObjectId;
                }
         
                var tags = obj.Select(kv => kv.Value.ToString());//?.Split(',')[1].TrimEnd(']')).ToList();

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
                        return new TagModel
                        {
                            tagId = tag.Id,
                            TagName = tag.Name,
                            Description = tag.Description,
                            AssociatedObject = objectName,
                            AssociatedObjectId = objectId,
                        };
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

        public static List<IEnumerable<KeyValuePair<string, object>>> GenerateTestData(
            IDefaultTaggingService taggingService,
            ITagFactory tagFactory,
            IAlertService alertService,
            IDynamicQueryBuilder<BaseTag, DefaultFilterCriteria> queryBuilder)
        {
            var testData = new List<IEnumerable<KeyValuePair<string, object>>>();

            var trans1 = new ExamplePersonTransaction { Sender = "John", Receiver = "Richard", Amount = 54123 };

            var fundTransferTag = tagFactory.CreateBaseTag("Transfering Funds", ExampleTags.FundsTransfer, null);
            taggingService.SetTag(trans1, fundTransferTag);

            var fundTransferTag2 = tagFactory.CreateBaseTag("Payment Expired", ExampleTags.PaymentExpired, null);
            taggingService.SetTag(trans1, fundTransferTag2);

            testData.Add(taggingService.GetAllTags(trans1)
                .Select(tag => new KeyValuePair<string, object>(tag.Name, tag)).ToList());

            var trans2 = new ExamplePersonTransaction { Sender = "Ed", Receiver = "Tim", Amount = 123 };

            var fundTransferTag3 = tagFactory.CreateBaseTag("Transfering Funds", ExampleTags.AccountActivity, "A transcation occured");
            taggingService.SetTag(trans2, fundTransferTag3);

            var fundTransferTag4 = tagFactory.CreateBaseTag("Transfering Funds", ExampleTags.AccountActivity, "A transcation occured");
            taggingService.SetTag(trans2, fundTransferTag4);

            testData.Add(taggingService.GetAllTags(trans2)
              .Select(tag => new KeyValuePair<string, object>(tag.Name, tag)).ToList());

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

    }
}
