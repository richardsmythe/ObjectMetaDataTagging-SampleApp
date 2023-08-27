using ObjectMetaDataTagging.Models;

namespace ObjectMetaDataTagging.Api.Models
{
    public class TagModel 
    {
        public string AssociatedObject { get; set; }
        public Guid AssociatedObjectId { get; set; }
        public string TagName { get; set; }
        public string Description { get; set; }

    }
}
