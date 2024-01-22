using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;

namespace ObjectMetaDataTagging.Api.Services
{
    public class CustomTaggingService<T> : InMemoryTaggingService<T>
        where T : BaseTag
    {
        public CustomTaggingService(
            TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> eventManager)
            : base(eventManager)
        {
        }

        public override async Task SetTagAsync(object o, T tag)
        {
            await base.SetTagAsync(o, tag);
           
            var tagFromEvent = _eventManager != null
                ? await _eventManager.RaiseTagAdded(new AsyncTagAddedEventArgs(o, tag))
                : null;

            if (tagFromEvent != null)
            {
                tagFromEvent.DateLastUpdated = DateTime.UtcNow;
                tagFromEvent.Parents.Add(tag.Id);

                await base.SetTagAsync(o, (T)tagFromEvent);
            }
        }   
    }
}
