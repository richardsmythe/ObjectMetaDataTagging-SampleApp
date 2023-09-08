using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IEventHandler<T>
    {
        BaseTag Handle(T eventArgs);
    }
}
