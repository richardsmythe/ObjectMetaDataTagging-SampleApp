using ObjectMetaDataTagging.Models.QueryModels;

namespace ObjectMetaDataTagging.Api.Models
{
    public class CustomFilter : DefaultFilterCriteria
    {
        public string Name { get; set; }
        public string Type{ get; set; }

        public CustomFilter(string name, string type)
            : base(null, null)
        {
            Name = name;
            Type = type;
        }
    }
}
