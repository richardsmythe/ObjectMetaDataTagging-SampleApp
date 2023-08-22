namespace ObjectMetaDataTagging.NewFolder
{
    public interface IDefaultTaggingService
    {
        // An interface version to all default tagging services if this ends up in a library of some kind
        void SetTag<T>(object o, T tag);
        bool UpdateTag<T>(object o, T oldTag, T newTag);
        T? GetTag<T>(object o, int tagIndex);
        bool HasTag<T>(object o, T tag);
        void RemoveAllTags(object o);
        void RemoveTag(object o, int tagIndex);
        IEnumerable<KeyValuePair<string, object>> GetAllTags(object o);
    }
}
