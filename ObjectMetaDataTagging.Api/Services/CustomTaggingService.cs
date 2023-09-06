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
            //Console.WriteLine($"Tag Id: {tag.Id}   TagName:  {tag.Name}  TagValue:  {tag.Value}");
            base.SetTag(o, tag);
        }
    }
}
