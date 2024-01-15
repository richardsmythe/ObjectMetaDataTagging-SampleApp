using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Helpers
{
    public class TagFactory : ITagFactory
    {
        public BaseTag CreateBaseTag(string name, object value, string description)
        {
            return new BaseTag(name, value, description);
        }
    }
}
