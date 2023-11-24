﻿using ObjectMetaDataTagging.Api.Models;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Api.Events
{
    public class SuspiciousTransactionEventArgs : EventArgs
    {
        public ExamplePersonTransaction Transaction { get; }

        public SuspiciousTransactionEventArgs(ExamplePersonTransaction transaction)
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
            if (args != null && args.TaggedObject is ExamplePersonTransaction transaction)
            {
                if (_alertService != null && transaction != null && _alertService.IsSuspiciousTransaction(transaction))
                {
                    //Console.WriteLine($"Number of AssociatedTags: {transaction.AssociatedTags.Count}");
                    if (!transaction.AssociatedTags.Any(tag => tag.Value.Equals(ExampleTags.Suspicious)))
                    {
                        var newTag = _tagFactory.CreateBaseTag("Suspicious Transfer", ExampleTags.Suspicious, "This object has been tagged as suspicious");
                        transaction.AssociatedTags.Add(newTag);
                        return Task.FromResult(newTag);
                    }                   
                }
            }
            return null;
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
