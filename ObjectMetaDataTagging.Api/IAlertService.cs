
using ObjectMetaDataTagging.Models;

namespace ObjectMetaDataTagging
{
    public interface IAlertService
    {
        bool IsSuspiciousTransaction(ExamplePersonTransaction transaction);
        void MarkAsSuspicious(ExamplePersonTransaction transaction, Guid tagId);
    }
}
