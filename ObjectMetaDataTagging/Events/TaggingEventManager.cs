using ObjectMetaDataTagging.Interfaces;

namespace ObjectMetaDataTagging.Events
{
    // Manage the event related to the tag
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

        public void RaiseTagAdded(TAdded e)
        {
            _addedHandler.Handle(e);
            TagAdded?.Invoke(this, e);
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
