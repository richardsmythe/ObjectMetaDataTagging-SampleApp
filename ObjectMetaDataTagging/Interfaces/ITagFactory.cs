using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface ITagFactory
    {
        /// <summary>
        /// Factory used to allow for creation of other types of tags if necessary
        /// </summary>
        BaseTag CreateBaseTag(string name, object value, string description);
    }
}
