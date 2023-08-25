
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.NewFolder;
using System.Linq;

namespace ObjectMetaDataTagging
{
    public class AlertService : IAlertService
    {
        private readonly IDefaultTaggingService _taggingService;

        public AlertService(IDefaultTaggingService taggingService)
        {
            _taggingService = taggingService;
        }
        public bool IsSuspiciousTransaction(ExamplePersonTransaction transaction)
        {
            return transaction.Amount > 1000 && !_taggingService.HasTag(transaction, ExampleTags.Suspicious);
        }

        public void MarkAsSuspicious(ExamplePersonTransaction transaction)
        {
            if (IsSuspiciousTransaction(transaction))
            {
                _taggingService.SetTag(transaction, ExampleTags.Suspicious);
            }
        }
    }
}
