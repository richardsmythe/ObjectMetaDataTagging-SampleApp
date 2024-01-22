using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Api.Events;

public class AlertService : IAlertService
{
    private readonly ITagFactory _tagFactory;

    public AlertService(ITagFactory tagFactory)
    {
        _tagFactory = tagFactory;
    }

    public bool IsSuspiciousTransaction(Transaction transaction)
    {
        return transaction.Amount > 3000;
    } 
}

