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
            var existingTag = _taggingService.GetTag(transaction, tagId);
            if (existingTag != null)
            {
                existingTag.Description = "Transaction marked as suspicious";               
                existingTag.Value = ExampleTags.Suspicious;
                _taggingService.UpdateTag(transaction, tagId, existingTag);
            }
            else
            {
                // if tag is not found...  
            }
        }
    }
}
