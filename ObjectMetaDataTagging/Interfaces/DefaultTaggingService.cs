using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Models.TagModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Interfaces
{
    public class DefaultTaggingService<T> : IDefaultTaggingService<T> where T : BaseTag
    {
        private readonly IDefaultTaggingService<T> _taggingService;

        public DefaultTaggingService(IDefaultTaggingService<T> taggingService)
        {
            _taggingService = taggingService ?? throw new ArgumentNullException(nameof(taggingService));
        }

        public Task SetTagAsync(object o, T tag) => _taggingService.SetTagAsync(o, tag);

        public Task<bool> UpdateTagAsync(object o, Guid tagId, T newTag) => _taggingService.UpdateTagAsync(o, tagId, newTag);

        IEnumerable<T> IDefaultTaggingService<T>.GetAllTags(object o) => _taggingService.GetAllTags(o);

        T? IDefaultTaggingService<T>.GetTag(object o, Guid tagId) => _taggingService.GetTag(o, tagId);

        public Task<bool> RemoveAllTagsAsync(object o) => _taggingService.RemoveAllTagsAsync(o);

        public Task<bool> RemoveTagAsync(object? o, Guid tagId) => _taggingService.RemoveTagAsync(o, tagId);

        public bool HasTag(object o, Guid tagId) => _taggingService.HasTag(o, tagId);

        public T? GetObjectByTag(Guid tagId) => _taggingService.GetObjectByTag(tagId);
    }
}
