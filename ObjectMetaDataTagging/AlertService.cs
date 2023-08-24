using ObjectMetaDataTagging.Extensions;
using ObjectMetaDataTagging.Models;
using System.Linq;

namespace ObjectMetaDataTagging
{
    public class AlertService : IAlertService
    {
        public bool IsSuspiciousTransaction(object obj)
        {
            if (obj is ExamplePersonTransaction transaction &&
                           transaction.Amount > 1000 &&
                           !transaction.HasTag(ExampleTags.Suspicious))
            {
                return true;
            }
            return false;
        }

        public void CheckForSuspiciousTransaction(object obj)
        {
            if (IsSuspiciousTransaction(obj))
            {
                obj.SetTag(ExampleTags.Suspicious);
            }
        }
    }
}
