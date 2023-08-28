using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;

namespace ObjectMetaDataTagging.Api.Events
{


    public class TagAddedHandler : IEventHandler<TagAddedEventArgs>
    {
        private readonly IAlertService _alertService;

        public TagAddedHandler(IAlertService alertService)
        {
            _alertService = alertService;
        }

        public void Handle(TagAddedEventArgs args)
        {
            if (args.TaggedObject is ExamplePersonTransaction transaction)
            {
                _alertService.MarkAsSuspicious(transaction, args.Tag.Id);
            }
        }
    }

    public class TagRemovedHandler : IEventHandler<TagRemovedEventArgs>
    {
        public void Handle(TagRemovedEventArgs args)
        {
            // tbc
        }
    }

    public class TagUpdatedHandler : IEventHandler<TagUpdatedEventArgs>
    {
        public void Handle(TagUpdatedEventArgs args)
        {
            // tbc
        }
    }
}
