using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Services
{
    public class DatabaseTaggingService<T, TDbContext> : IDefaultTaggingService<T>
        where T : BaseTag
        where TDbContext : ITaggingDatabaseContext<T>
    {
        private readonly TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> _eventManager;
        private readonly TDbContext _databaseContext;

        public DatabaseTaggingService(
            TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> eventManager,
            TDbContext databaseContext)
        {
            if (!(databaseContext is ITaggingDatabaseContext<T>))
            {
                throw new ArgumentException($"{nameof(databaseContext)} must implement ITaggingDatabaseContext<{typeof(T).Name}>");
            }

            _eventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        public async Task<IEnumerable<T>> GetAllTags(object o)
        {
            if (o == null)
                throw new ArgumentNullException(nameof(o));

            var objectId = GetObjectId(o);
            return await _databaseContext.GetTagsForObject(objectId);
        }

        public async Task<T?> GetTag(object o, Guid tagId)
        {
            if (o == null)
                throw new ArgumentNullException(nameof(o));

            var objectId = GetObjectId(o);
            return await _databaseContext.GetTagById(objectId, tagId);
        }

        public bool HasTag(object o, Guid tagId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveAllTagsAsync(object o)
        {
            if (o == null)
                throw new ArgumentNullException(nameof(o));

            var objectId = GetObjectId(o);
            await _databaseContext.DeleteAllTagsForObject(objectId);
            return true;
        }

        public async Task<bool> RemoveTagAsync(object? o, Guid tagId)
        {
            if (o == null)
                throw new ArgumentNullException(nameof(o));

            var objectId = GetObjectId(o);
            return await _databaseContext.DeleteTagForObject(objectId, tagId);
        }

        public async Task SetTagAsync(object o, T tag)
        {
            if (tag == null || o == null)
                return;

            var objectId = GetObjectId(o);

            var tagFromEvent = _eventManager != null
                ? await _eventManager.RaiseTagAdded(new AsyncTagAddedEventArgs(o, tag))
                : null;

            if (tagFromEvent != null)
            {
                tagFromEvent.AssociatedParentObjectId = objectId;
                await _databaseContext.AddTagForObject(objectId, (T)tagFromEvent);
            }

            tag.AssociatedParentObjectId = objectId;
            await _databaseContext.AddTagForObject(objectId, tag);
        }

        public async Task<bool> UpdateTagAsync(object o, Guid tagId, T newTag)
        {
            var objectId = GetObjectId(o);
            return await _databaseContext.UpdateTagForObject(objectId, tagId, newTag);
        }

        private Guid GetObjectId(object o)
        {
            if (o != null)
            {
                var idProperty = o.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    var idValue = idProperty.GetValue(o);
                    if (idValue is Guid guidValue)
                    {
                        return guidValue;
                    }
                }
            }
            return Guid.Empty;
        }

        public async Task<T?> GetObjectByTag(Guid tagId)
        {
            var objectsWithTag = await _databaseContext.GetObjectsByTagId(tagId);
            return objectsWithTag.FirstOrDefault();
        }
    }

    public interface ITaggingDatabaseContext<T> where T : BaseTag
    {
        // the ITaggingDatabaseContext<T> interface is independent of Entity Framework.
        // so developers can implement it according to their specific data access needs.
        Task<IEnumerable<T>> GetObjectsByTagId(Guid tagId);
        Task<IEnumerable<T>> GetTagsForObject(Guid objectId);
        Task<T> GetTagById(Guid objectId, Guid tagId);
        Task DeleteAllTagsForObject(Guid objectId);
        Task<bool> DeleteTagForObject(Guid objectId, Guid tagId);
        Task AddTagForObject(Guid objectId, T tag);
        Task<bool> UpdateTagForObject(Guid objectId, Guid tagId, T modifiedTag);
    }
}
