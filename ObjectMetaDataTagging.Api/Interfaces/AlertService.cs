using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Api.Events;

public class AlertService : IAlertService
{
    //private readonly IDefaultTaggingService _taggingService;
    private readonly ITagFactory _tagFactory;

    public AlertService(/*IDefaultTaggingService taggingService,*/ ITagFactory tagFactory)
    {
        //_taggingService = taggingService;
        _tagFactory = tagFactory;
    }

    public bool IsSuspiciousTransaction(ExamplePersonTransaction transaction)
    {
        return transaction.Amount > 1000;
    }

    public void MarkAsSuspicious(ExamplePersonTransaction transaction, Guid tagId)
    {
        //if (IsSuspiciousTransaction(transaction))
        //{
        //    var existingTags = _taggingService.GetAllTags(transaction);
        //    if (existingTags != null)
        //    {
        //        //Check if a suspicious tag is already set
        //        var suspiciousTagExists = existingTags.Any(tag => Equals(tag.Value, ExampleTags.Suspicious));

        //        if (!suspiciousTagExists)
        //        {
        //            var newTag = _tagFactory.CreateBaseTag(
        //                "Suspicious Transfer",
        //                ExampleTags.Suspicious,
        //                "This object has been tagged as suspicious");

        //            _taggingService.SetTag(transaction, newTag);
        //        }
        //    }
        //}
    }
}

