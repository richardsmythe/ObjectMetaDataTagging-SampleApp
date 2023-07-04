namespace ObjectMetaDataTagging.Api.Models
{
    public class Frame
    {
        public Guid Id { get; set; }
        public string FrameName { get; set; }
        public ICollection<ObjectModel> ObjectModel{ get; set;}
        public ICollection<TagModel> TagModel{ get; set;}

    }
}
