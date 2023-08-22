using static ObjectMetaDataTagging.Extensions.ObjectTaggingExtensions;

namespace ObjectMetaDataTagging.Extensions
{
    // Manage the event related to the tag
    public class TaggingEventManager
    {
        private readonly IAlertService _alertServiceNEW; // used for demo purposes
        private IAlertService _alertService; // this is used for the old version as a demo
        public event EventHandler<TagAddedEventArgs>? TagAdded; // To do: TagRemoved, TagUpdated etc 

        public TaggingEventManager(IAlertService alertServiceNEW)
        {
            _alertServiceNEW = alertServiceNEW; // TO DO clean this up
            _alertService = new AlertService(); // old way of passing the alertService, left in for demo purposes
            TagAdded += HandleTagAdded!;
        }

        public void RaiseTagAdded(TagAddedEventArgs e)
        {
            TagAdded?.Invoke(this, e);
        }

        private void HandleTagAdded(object sender, TagAddedEventArgs e)
        {
            _alertService.CheckForSuspiciousTransaction(e.Object);
        }
    }
}
