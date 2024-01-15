using ObjectMetaDataTagging.Models.TagModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface ITagMapper<T>
    { 
        Task<T> MapTagsBetweenTypes(object sourceObject);
    }
}
