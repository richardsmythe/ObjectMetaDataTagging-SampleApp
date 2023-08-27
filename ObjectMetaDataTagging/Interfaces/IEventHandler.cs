namespace ObjectMetaDataTagging.Interfaces
{
    public interface IEventHandler<T>
    {
        void Handle(T eventArgs);
    }
}
