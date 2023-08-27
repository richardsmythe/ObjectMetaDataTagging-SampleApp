using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;

namespace ObjectMetaDataTagging.Api.Events
{


    public class TestAddedHandler : IEventHandler<TagAddedEventArgs>
    {
        private readonly IAlertService _alertService;

        public TestAddedHandler(IAlertService alertService)
        {
            _alertService = alertService;
        }

        public void Handle(TagAddedEventArgs args)
        {
            if (args.TaggedObject is ExamplePersonTransaction transaction)
            {
                _alertService.MarkAsSuspicious(transaction);
            }
        }
    }

    public class TestRemovedHandler : IEventHandler<TagRemovedEventArgs>
    {
        public void Handle(TagRemovedEventArgs args)
        {
            // tbc
        }
    }

    public class TestUpdatedHandler : IEventHandler<TagUpdatedEventArgs>
    {
        public void Handle(TagUpdatedEventArgs args)
        {
            // tbc
        }
    }
}
