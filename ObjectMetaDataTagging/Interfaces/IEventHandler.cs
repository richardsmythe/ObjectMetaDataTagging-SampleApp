using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IEventHandler<T>
    {
        void Handle(T eventArgs);
    }
}
