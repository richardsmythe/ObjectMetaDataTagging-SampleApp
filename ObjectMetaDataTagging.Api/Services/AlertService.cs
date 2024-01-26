using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Interfaces;


public class AlertService : IAlertService
{
    private readonly ITagFactory _tagFactory;

    public AlertService(ITagFactory tagFactory)
    {
        _tagFactory = tagFactory;
    }

    public bool IsSuspiciousTransaction(Transaction transaction)
    {
        return transaction.Amount > 2000;
    } 
}

