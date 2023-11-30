﻿using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDefaultTaggingService<T>
        where T : BaseTag
    {
        Task SetTagAsync(object o, T tag);
        Task<bool> UpdateTagAsync(object o, Guid tagId, T newTag);
        IEnumerable<T> GetAllTags(object o);
        T? GetTag(object o, Guid tagId);
        Task<bool> RemoveAllTagsAsync(object o);
        Task<bool> RemoveTagAsync(object? o, Guid tagId);
        bool HasTag(object o, Guid tagId);
        T? GetObjectByTag(Guid tagId);
    }
}
