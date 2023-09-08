using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDefaultTaggingService
    {
        void SetTag(object o, BaseTag tag);
        bool UpdateTag(object o, Guid tagId, BaseTag newTag);
        IEnumerable<BaseTag> GetAllTags(object o);
        BaseTag? GetTag(object o, Guid tagId);
        void RemoveAllTags(object o);
        bool RemoveTag(object o, Guid tagId);        
        bool HasTag(object o, Guid tagId);


    }
}
