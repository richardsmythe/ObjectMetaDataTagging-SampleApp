using Microsoft.AspNetCore.Mvc;
using ObjectMetaDataTagging.Api.Models;
using System.Reflection;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Api.Events;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Models.QueryModels;

namespace ObjectMetaDataTagging.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly IDynamicQueryBuilder<DefaultFilterCriteria> _dynamicQueryBuilder;
        private readonly IDefaultTaggingService _taggingService;
        private readonly ITagFactory _tagFactory;
        private readonly IAlertService _alertService;
        private readonly TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs> _eventManager;

        public TagController(
            IDynamicQueryBuilder<DefaultFilterCriteria> dynamicQueryBuilder,
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

        [HttpGet]
        public IActionResult GetObjectsAndTags()
        {
            var testData = GenerateTestData(_taggingService, _tagFactory, _alertService, _dynamicQueryBuilder);

            var objectModels = new List<ObjectModel>();
            var tagModels = new List<TagModel>();
            var objectName = "";
            foreach (var obj in testData)
            {
                // gets type --> obj.First().Key.GetType().Name.ToString(); 
                if (obj.First().Value is BaseTag baseTag)
                {
                    objectName = baseTag.AssociatedParentObjectName.ToString();
                }
                var objectId = Guid.NewGuid();
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
                Id = Guid.NewGuid(),
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
            IDynamicQueryBuilder<DefaultFilterCriteria> queryBuilder)
        {
            var testData = new List<IEnumerable<KeyValuePair<string, object>>>();

            var trans1 = new ExamplePersonTransaction { Sender = "John", Receiver = "Richard", Amount = 2531 };

            var fundTransferTag = tagFactory.CreateBaseTag("Transfering Funds", ExampleTags.FundsTransfer, null);
            taggingService.SetTag(trans1, fundTransferTag);

            var fundTransferTag2 = tagFactory.CreateBaseTag("Payment Expired", ExampleTags.PaymentExpired, null);
            taggingService.SetTag(trans1, fundTransferTag2);

            //testData.Add(taggingService.GetAllTags(trans1)
            //    .Select(tag => new KeyValuePair<string, object>(tag.Name, tag)).ToList());

            /////////// Dynamic Filter Test /////////////

            var trans2 = new ExamplePersonTransaction { Sender = "Someone", Receiver = "Someone Else", Amount = 34 };
            var filterTagTest1 = tagFactory.CreateBaseTag("Payment Expired", ExampleTags.PaymentExpired, null);
            var filterTagTest2 = tagFactory.CreateBaseTag("Payment Expired", ExampleTags.PaymentExpired, null);
            var filterTagTest3 = tagFactory.CreateBaseTag("Transfer Funds", ExampleTags.FundsTransfer, null);
            var filterTagTest4 = tagFactory.CreateBaseTag("Account Activity", ExampleTags.AccountActivity, null);
            taggingService.SetTag(trans2, filterTagTest1);
            taggingService.SetTag(trans2, filterTagTest2);
            taggingService.SetTag(trans2, filterTagTest3);
            taggingService.SetTag(trans2, filterTagTest4);

            testData.Add(taggingService.GetAllTags(trans2)
               .Select(tag => new KeyValuePair<string, object>(tag.Name, tag)).ToList());

            var filters = new List<DefaultFilterCriteria>
            {
                new CustomFilter("Payment Expired", "ExampleTags")
            };

            var filtered = queryBuilder.BuildDynamicQuery<BaseTag>(trans2.AssociatedTags, filters, LogicalOperator.AND);


            return testData;
        }

    }
}
