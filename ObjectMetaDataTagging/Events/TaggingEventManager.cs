using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Events
{
    /// <summary>
    /// Manages events related to tagging actions, allowing event handlers to be attached to tagging events.
    /// </summary>
    /// <typeparam name="TAdded">The type of event arguments for tag added events.</typeparam>
    /// <typeparam name="TRemoved">The type of event arguments for tag removed events.</typeparam>
    /// <typeparam name="TUpdated">The type of event arguments for tag updated events.</typeparam>

    public class TaggingEventManager<TAdded, TRemoved, TUpdated>
        where TAdded : TagAddedEventArgs
        where TRemoved : TagRemovedEventArgs
        where TUpdated : TagUpdatedEventArgs
    {

        public event EventHandler<TagAddedEventArgs>? TagAdded;
        public event EventHandler<TagRemovedEventArgs>? TagRemoved;
        public event EventHandler<TagUpdatedEventArgs>? TagUpdated;

        private readonly IEventHandler<TAdded> _addedHandler;
        private readonly IEventHandler<TRemoved> _removedHandler;
        private readonly IEventHandler<TUpdated> _updatedHandler;


        public TaggingEventManager(IEventHandler<TAdded> addedHandler,
                               IEventHandler<TRemoved> removedHandler,
                               IEventHandler<TUpdated> updatedHandler)
        {
            _addedHandler = addedHandler;
            _removedHandler = removedHandler;
            _updatedHandler = updatedHandler;
        }

        public BaseTag RaiseTagAdded(TAdded e)
        {
           var result = _addedHandler.Handle(e);
            TagAdded?.Invoke(this, e);
            return result;
        }
        public void RaiseTagRemoved(TRemoved e)
        {
            _removedHandler.Handle(e);
            TagRemoved?.Invoke(this, e);
        }
        public void RaiseTagUpdated(TUpdated e)
        {
            _updatedHandler.Handle(e);
            TagUpdated?.Invoke(this, e);
        }
      
    }
}
