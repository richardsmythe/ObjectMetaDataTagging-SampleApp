using ObjectMetaDataTagging.Models;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IEventHandler<T>
    {
        BaseTag Handle(T eventArgs);
    }
}
