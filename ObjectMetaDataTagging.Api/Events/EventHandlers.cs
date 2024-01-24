using ObjectMetaDataTagging.Api.Models;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;
using System;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Api.Events
{
    public class SuspiciousTransactionEventArgs : EventArgs
    {
        public Transaction Transaction { get; }

        public SuspiciousTransactionEventArgs(Transaction transaction)
        {
            Transaction = transaction;
        }
    }

    public class TagAddedHandler
    {
        private readonly IAlertService _alertService;
        private readonly ITagFactory _tagFactory;
        private readonly ObjectMetaDataTaggingFacade<BaseTag> _taggingFacade;

        public TagAddedHandler(IAlertService alertService, ITagFactory tagFactory, ObjectMetaDataTaggingFacade<BaseTag> taggingFacade)
        {
            _alertService = alertService;
            _tagFactory = tagFactory;
            _taggingFacade = taggingFacade;
        }

        public async Task<BaseTag> HandleTagAddedAsync(object o, BaseTag tag)
        {
            try
            {
                if (o is Transaction transaction && _alertService != null && _alertService.IsSuspiciousTransaction(transaction))
                {
                    var newTag = _tagFactory.CreateBaseTag("Suspicious Transfer", ExampleTags.Suspicious, "This object has been tagged as suspicious");
                    await _taggingFacade.SetTagAsync(transaction, newTag);
                    return newTag;
                }

                // handles the case where no suspicious tag is added, and just continues
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in HandleTagAddedAsync: {ex}");
                throw;
            }
        }
    }
}
