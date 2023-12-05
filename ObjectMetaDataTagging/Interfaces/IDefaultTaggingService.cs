using ObjectMetaDataTagging.Models.TagModels;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDefaultTaggingService<T>
        where T : BaseTag
    {
        Task SetTagAsync(object o, T tag);
        Task<bool> UpdateTagAsync(object o, Guid tagId, T newTag);
        Task<IEnumerable<T>> GetAllTags(object o);
        Task<T>? GetTag(object o, Guid tagId);
        Task<bool> RemoveAllTagsAsync(object o);
        Task<bool> RemoveTagAsync(object? o, Guid tagId);
        bool HasTag(object o, Guid tagId);
        Task<T?> GetObjectByTag(Guid tagId);
    }
}
