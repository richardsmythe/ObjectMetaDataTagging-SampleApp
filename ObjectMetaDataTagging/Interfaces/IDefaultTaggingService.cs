using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDefaultTaggingService<T>
        where T : BaseTag
    {
        Task SetTag(object o, T tag);
        Task<bool> UpdateTag(object o, Guid tagId, T newTag);
        IEnumerable<T> GetAllTags(object o);
        T? GetTag(object o, Guid tagId);
        void RemoveAllTags(object o);
        Task<bool> RemoveTag(object? o, Guid tagId);
        bool HasTag(object o, Guid tagId);
        object? GetObjectByTag(Guid tagId);
    }
}
