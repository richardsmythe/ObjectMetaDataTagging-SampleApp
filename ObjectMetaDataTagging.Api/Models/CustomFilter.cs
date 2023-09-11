using ObjectMetaDataTagging.Models.QueryModels;

namespace ObjectMetaDataTagging.Api.Models
{
    public class CustomFilter : DefaultFilterCriteria
    {
        public CustomFilter(string name, string type)
            : base(name, type)
        {
           
        }
    }

}
