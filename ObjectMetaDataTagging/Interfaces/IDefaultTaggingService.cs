using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.NewFolder
{
    public interface IDefaultTaggingService
    {
        // An interface version to all default tagging services if this ends up in a library of some kind
        void SetTag<T>(object target, T tag);
        T? GetTag<T>(object target, int tagIndex);
        bool HasTag<T>(object target, T tag);
        void RemoveAllTags(object target);
        void RemoveTag(object target, int tagIndex);
        IEnumerable<KeyValuePair<string, object>> GetAllTags(object target);
    }
}
