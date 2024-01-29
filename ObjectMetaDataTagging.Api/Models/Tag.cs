using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Api.Models
{
    public class Tag : BaseTag
    {
        public string SomeField { get; set; }
        public string AnotherField { get; set; }    
        //public Tag(string name, object value, string description = null) : base(name, value, description)
        //{
        //}
    }
}
