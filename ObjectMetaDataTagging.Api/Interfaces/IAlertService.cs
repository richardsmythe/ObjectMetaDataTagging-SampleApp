
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IAlertService
    {
        bool IsSuspiciousTransaction(Transaction transaction);

    }
}
