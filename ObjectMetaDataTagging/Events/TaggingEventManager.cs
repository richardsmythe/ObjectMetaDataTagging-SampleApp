using static ObjectMetaDataTagging.Extensions.ObjectTaggingExtensions;

namespace ObjectMetaDataTagging.Events
{
    // Manage the event related to the tag
    public class TaggingEventManager
    {
        private readonly IAlertService _alertServiceNEW; // used for demo purposes
        private IAlertService _alertService; // this is used for the old version as a demo

        public event EventHandler<TagAddedEventArgs>? TagAdded; // To do: TagRemoved, TagUpdated etc 
        public event EventHandler<TagRemovedEventArgs>? TagRemoved;
        public event EventHandler<TagUpdatedEventArgs>? TagUpdated;
        public TaggingEventManager(IAlertService alertServiceNEW)
        {
            _alertServiceNEW = alertServiceNEW; // TO DO clean this up
            _alertService = new AlertService(); // old way of passing the alertService, left in for demo purposes
            
            TagAdded += HandleTagAdded!;
            TagRemoved += HandleTagRemoved!;
        }

        public void RaiseTagAdded(TagAddedEventArgs e)
        {
            TagAdded?.Invoke(this, e);
        }
        public void RaiseTagRemoved (TagRemovedEventArgs e)
        {
            TagRemoved?.Invoke(this, e);
        }
        public void RaiseTagUpdated(TagUpdatedEventArgs e)
        {
            TagUpdated?.Invoke(this, e);
        }
        private void HandleTagAdded(object sender, TagAddedEventArgs e)
        {
            _alertService.CheckForSuspiciousTransaction(e.TaggedObject);
        }

        private void HandleTagRemoved(object sender, TagRemovedEventArgs e)
        {
            Console.WriteLine($"Removed tag: " + e.Tag + "from object: " + e.TaggedObject);          
        }

        private void HandleTagUpdated(object sender, TagUpdatedEventArgs e)
        {
            Console.WriteLine($"Updated tag: " + e.Tag + "on object: " + e.TaggedObject);
        }
    }
}
