using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging;

public class AlertService : IAlertService
{
    private readonly IDefaultTaggingService _taggingService;

    public AlertService(IDefaultTaggingService taggingService)
    {
        _taggingService = taggingService;
    }

    public bool IsSuspiciousTransaction(ExamplePersonTransaction transaction)
    {
        return transaction.Amount > 1000;
    }

    public void MarkAsSuspicious(ExamplePersonTransaction transaction, Guid tagId)
    {
        if (IsSuspiciousTransaction(transaction))
        {
            var existingTags = _taggingService.GetAllTags(transaction);
            if (existingTags != null)
            {
                // Check if a suspicious tag is already set
                var suspiciousTagExists = existingTags.Any(tag => Equals(tag.Value, ExampleTags.Suspicious));

                if (!suspiciousTagExists)
                {
                    var newTag = new BaseTag("Suspicious Transfer", ExampleTags.Suspicious);
                    newTag.Description = "This object has been tagged as suspicious";
                    _taggingService.SetTag(transaction, newTag);
                }
            }
        }
    }
}
