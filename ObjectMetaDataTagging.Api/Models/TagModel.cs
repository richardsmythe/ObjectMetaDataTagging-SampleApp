using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Api.Models
{
    public class TagModel 
    {
        public Guid tagId { get; set; }
        public string AssociatedObject { get; set; }
        public Guid AssociatedObjectId { get; set; }
        public string TagName { get; set; }
        public string Description { get; set; }

        public ICollection<BaseTag> ChildTags { get; set; }
 

    }
}
