using ObjectMetaDataTagging.Api.Models;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;

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

    public class TagAddedHandler : IAsyncEventHandler<AsyncTagAddedEventArgs>
    {
        private readonly IAlertService _alertService;
        private readonly ITagFactory _tagFactory;

        public TagAddedHandler(IAlertService alertService, ITagFactory tagFactory)
        {
            _alertService = alertService;
            _tagFactory = tagFactory;

        }

        public Task<BaseTag> HandleAsync(AsyncTagAddedEventArgs args)
        {
            try
            {
                if (args != null && args.TaggedObject is Transaction transaction)
                {
                    if (_alertService != null && transaction != null && _alertService.IsSuspiciousTransaction(transaction))
                    {
                        if (!transaction.AssociatedTags.Any(tag => tag.Value.Equals(ExampleTags.Suspicious)))
                        {
                            var newTag = _tagFactory.CreateBaseTag("Suspicious Transfer", ExampleTags.Suspicious, "This object has been tagged as suspicious");
                            transaction.AssociatedTags.Add(newTag);
                            return Task.FromResult(newTag);
                        }
                    }
                }
                // handles the case where no suspicious tag is added, and just continues
                return Task.FromResult<BaseTag>(null);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Exception in HandleAsync: {ex}");              
                throw;
            }
        }

    }

    public class TagRemovedHandler : IAsyncEventHandler<AsyncTagRemovedEventArgs>
    {
        public Task<BaseTag> HandleAsync(AsyncTagRemovedEventArgs args)
        {
            // tbc
            return null;
        }
    }

    public class TagUpdatedHandler : IAsyncEventHandler<AsyncTagUpdatedEventArgs>
    {
        public Task<BaseTag> HandleAsync(AsyncTagUpdatedEventArgs args)
        {
            // tbc
            return null;
        }
    }
}
