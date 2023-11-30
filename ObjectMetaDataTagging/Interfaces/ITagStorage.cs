using ObjectMetaDataTagging.Models.TagModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface ITagStorage
    {
        Task SetTagAsync(Object o, BaseTag tag);
        Task RemoveTagAsync(Object o, BaseTag tag);
        Task<BaseTag> GetTag(Guid tagId);
        Task<IEnumerable<BaseTag>> GetAllTags(Guid objectId);
    }
}
