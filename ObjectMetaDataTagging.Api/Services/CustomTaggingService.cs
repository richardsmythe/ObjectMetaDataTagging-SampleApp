using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;
using System;
using System.Threading.Tasks;

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
                tagFromEvent.AssociatedParentObjectId = tag.AssociatedParentObjectId;
                tagFromEvent.AssociatedParentObjectName = tag.AssociatedParentObjectName;

                await base.SetTagAsync(o, (T)tagFromEvent);
            }
        }   
    }
}
