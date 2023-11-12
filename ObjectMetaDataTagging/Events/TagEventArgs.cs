using System;
using System.Threading.Tasks;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Events
{
    public class AsyncTagAddedEventArgs : EventArgs
    {
        public object TaggedObject { get; }
        public BaseTag Tag { get; }

        public AsyncTagAddedEventArgs(object taggedObject, BaseTag tag)
        {
            TaggedObject = taggedObject ?? throw new ArgumentNullException(nameof(taggedObject));
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        }
    }

    public class AsyncTagRemovedEventArgs : EventArgs
    {
        public object TaggedObject { get; }
        public object Tag { get; }

        public AsyncTagRemovedEventArgs(object taggedObject, object tag)
        {
            TaggedObject = taggedObject ?? throw new ArgumentNullException(nameof(taggedObject));
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        }
    }

    public class AsyncTagUpdatedEventArgs : EventArgs
    {
        public object TaggedObject { get; }
        public BaseTag OldTag { get; }
        public BaseTag NewTag { get; }

        public AsyncTagUpdatedEventArgs(object taggedObject, BaseTag oldTag, BaseTag newTag)
        {
            TaggedObject = taggedObject ?? throw new ArgumentNullException(nameof(taggedObject));
            OldTag = oldTag ?? throw new ArgumentNullException(nameof(oldTag));
            NewTag = newTag ?? throw new ArgumentNullException(nameof(newTag));
        }
    }
}
