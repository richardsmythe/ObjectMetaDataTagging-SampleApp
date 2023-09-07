using ObjectMetaDataTagging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Interfaces
{
    public class TagFactory : ITagFactory
    {
        public BaseTag CreateBaseTag(string name, object value, string description)
        {            
            return new BaseTag(name, value, description);
        }
    }
}
