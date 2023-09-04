using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models;

namespace ObjectMetaDataTagging.Api.Services
{
    public class CustomTaggingService : DefaultTaggingService
    {
        public CustomTaggingService(
            TaggingEventManager<TagAddedEventArgs, TagRemovedEventArgs, TagUpdatedEventArgs> eventManager) 
                : base(eventManager)
        {
        }

        public override void SetTag(object o, BaseTag tag)
        {
            if (o is ExamplePersonTransaction transaction)
            {
                transaction.AssociatedTags.Add(tag);
            }
            base.SetTag(o, tag);
        }
    }
}
