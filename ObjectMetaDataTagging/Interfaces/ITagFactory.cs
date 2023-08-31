using ObjectMetaDataTagging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface ITagFactory
    {
        // Factory used to allow for creation of other types of tags if necessary
        BaseTag CreateBaseTag(string name, object value, string description);
    }
}
