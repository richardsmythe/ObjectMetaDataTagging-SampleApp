using ObjectMetaDataTagging.Models;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Api.Models
{
    public class TagModel 
    {
        public Guid TagId { get; set; }
        public string AssociatedObjectName { get; set; }
        public List<Guid> ParentIds { get; set; } = new List<Guid>();
        public string TagName { get; set; }
        public string Description { get; set; }

        public List<TagModel> ChildTags { get; set; }
 

    }
}
