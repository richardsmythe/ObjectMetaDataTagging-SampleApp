namespace ObjectMetaDataTagging.Models
{
    public class BaseTag
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public DateTime DateCreated{ get; private set; }
        public object Value { get; set; }

        public BaseTag(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Id = Guid.NewGuid();
            Name = name;
            Value = value;  
            DateCreated = DateTime.UtcNow;
        }
    }
}
