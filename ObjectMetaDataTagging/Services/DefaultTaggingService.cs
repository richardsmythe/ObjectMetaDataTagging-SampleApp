using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Utilities;

namespace ObjectMetaDataTagging.Services
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

        Task<IEnumerable<T>> IDefaultTaggingService<T>.GetAllTags(object o) => _taggingService.GetAllTags(o);

        Task<T>? IDefaultTaggingService<T>.GetTag(object o, Guid tagId) => _taggingService.GetTag(o, tagId);

        public Task<bool> RemoveAllTagsAsync(object o) => _taggingService.RemoveAllTagsAsync(o);

        public Task<bool> RemoveTagAsync(object? o, Guid tagId) => _taggingService.RemoveTagAsync(o, tagId);

        public bool HasTag(object o, Guid tagId) => _taggingService.HasTag(o, tagId);

        public object? GetObjectByTag(Guid tagId) => _taggingService.GetObjectByTag(tagId);

        public Task<List<GraphNode>> GetObjectGraph() => _taggingService.GetObjectGraph();
    }
}
