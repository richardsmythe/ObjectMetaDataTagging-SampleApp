using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Services
{
    public class DatabaseTaggingService<T> : IDefaultTaggingService<T> where T : BaseTag
    {
        public Task<IEnumerable<T>> GetAllTags(object o)
        {
            throw new NotImplementedException();
        }

        public T? GetObjectByTag(Guid tagId)
        {
            throw new NotImplementedException();
        }

        public Task<T>? GetTag(object o, Guid tagId)
        {
            throw new NotImplementedException();
        }

        public bool HasTag(object o, Guid tagId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveAllTagsAsync(object o)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveTagAsync(object? o, Guid tagId)
        {
            throw new NotImplementedException();
        }

        public Task SetTagAsync(object o, T tag)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateTagAsync(object o, Guid tagId, T newTag)
        {
            throw new NotImplementedException();
        }
    }
}
