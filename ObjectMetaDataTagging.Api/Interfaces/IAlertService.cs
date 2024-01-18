
using ObjectMetaDataTagging.Models;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IAlertService
    {
        bool IsSuspiciousTransaction(Transaction transaction);

    }
}
