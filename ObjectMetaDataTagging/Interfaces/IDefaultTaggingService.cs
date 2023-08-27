namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDefaultTaggingService
    {
        void SetTag<T>(object o, T tag);
        bool UpdateTag<T>(object o, T oldTag, T newTag);
        IEnumerable<KeyValuePair<string, object>> GetAllTags(object o);
        T? GetTag<T>(object o, int tagIndex);
        bool HasTag<T>(object o, T tag);
        void RemoveAllTags(object o);
        void RemoveTag(object o, int tagIndex);
    }
}
