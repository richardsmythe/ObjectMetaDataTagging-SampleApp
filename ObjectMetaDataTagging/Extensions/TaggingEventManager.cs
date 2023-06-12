using static ObjectMetaDataTagging.Extensions.ObjectTaggingExtensions;

namespace ObjectMetaDataTagging.Extensions
{
    // Manage the event related to the tag
    public class TaggingEventManager
    {
        private IAlertService _alertService;
        public event EventHandler<TagAddedEventArgs>? TagAdded;

        public TaggingEventManager()
        {
            _alertService = new AlertService();
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
