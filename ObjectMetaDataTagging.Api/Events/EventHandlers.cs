using ObjectMetaDataTagging.Api.Models;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;

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

    public class TagAddedHandler : IEventHandler<TagAddedEventArgs>
    {
        private readonly IAlertService _alertService;
        private readonly ITagFactory _tagFactory;

        public TagAddedHandler(IAlertService alertService, ITagFactory tagFactory)
        {
            _alertService = alertService;
            _tagFactory = tagFactory;

        }

        public BaseTag Handle(TagAddedEventArgs args)
        {
            if (args.TaggedObject is ExamplePersonTransaction transaction)
            {

                if (_alertService.IsSuspiciousTransaction(transaction))
                {
                    // Check if there are any tags with the value ExampleTags.Suspicious
                    if (!transaction.AssociatedTags.Any(tag => (ExampleTags)tag.Value == ExampleTags.Suspicious))
                    {
                        var newTag = _tagFactory.CreateBaseTag("Suspicious Transfer", ExampleTags.Suspicious, "This object has been tagged as suspicious");
                        return newTag;
                    }
                }

            }
            return null;
        }

    }

    public class TagRemovedHandler : IEventHandler<TagRemovedEventArgs>
    {
        public BaseTag Handle(TagRemovedEventArgs args)
        {
            // tbc
            return null;
        }
    }

    public class TagUpdatedHandler : IEventHandler<TagUpdatedEventArgs>
    {
        public BaseTag Handle(TagUpdatedEventArgs args)
        {
            // tbc
            return null;
        }
    }
}
