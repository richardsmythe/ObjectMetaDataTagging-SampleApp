
namespace ObjectMetaDataTagging.Events
{
    public class TagAddedEventArgs : EventArgs
    {

        public object TaggedObject { get; }
        public object Tag { get; }

        public TagAddedEventArgs(object taggedObject, object tag)
        {
            TaggedObject = taggedObject ?? throw new ArgumentNullException(nameof(taggedObject));
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        }
    }

    public class TagRemovedEventArgs : EventArgs
    {
        public object TaggedObject { get; }
        public object Tag { get; }

        public TagRemovedEventArgs(object taggedObject, object tag)
        {
            TaggedObject = taggedObject ?? throw new ArgumentNullException(nameof(taggedObject));
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        }
    }

    public class TagUpdatedEventArgs : EventArgs
    {
        public object TaggedObject { get; }
        public object OldTag { get; }
        public object NewTag { get; }

        public TagUpdatedEventArgs(object taggedObject, object oldTag, object newTag)
        {
            TaggedObject = taggedObject ?? throw new ArgumentNullException(nameof(taggedObject));
            OldTag = oldTag ?? throw new ArgumentNullException(nameof(oldTag));
            NewTag = newTag ?? throw new ArgumentNullException(nameof(newTag));
        }
    }
}
