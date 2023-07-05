namespace ObjectMetaDataTagging.Api.Models
{
    public class Frame
    {
        public Guid Id { get; set; }
        public string FrameName { get; set; }
        public ICollection<ObjectModel> ObjectData{ get; set;}
        public ICollection<TagModel> TagData{ get; set;}

    }
}
