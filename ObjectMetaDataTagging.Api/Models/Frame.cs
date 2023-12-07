namespace ObjectMetaDataTagging.Api.Models
{
    public class Frame
    {
        public int Id { get; set; }
        public string Origin { get; set; }
        public ICollection<ObjectModel> ObjectData { get; set; }
        public ICollection<TagModel> TagData { get; set; }        

    }
}
