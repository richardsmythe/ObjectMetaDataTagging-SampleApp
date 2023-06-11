using ObjectMetaDataTagging.Extensions;
using ObjectMetaDataTagging.Models;

namespace ObjectMetaDataTagging
{
    public class AlertService : IAlertService
    {
        public void CheckForSuspiciousTransaction(object obj)
        {
            if (obj is PersonTransaction transaction && transaction.Amount > 1000)
            {
                transaction.SetTag("Suspicious");
            }
        }       
    }
}
