namespace ObjectMetaDataTagging.Models.TagModels
{
    /// <summary>
    /// A base tag providing basic tag object properties that potential derived classes
    /// may require.
    /// </summary>
    public class BaseTag
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; private set; }
        public DateTime? DateLastUpdated { get; set; }
        public string Description { get; set; }
        public object Value { get; set; }
        public object AssociatedParentObjectName { get; set; }
        public Guid AssociatedParentObjectId { get; set; }
        public string Type { get; private set; }
        public List<BaseTag> ChildTags { get; private set; }

        public BaseTag(string name, object value, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            SetValue(value);
            DateCreated = DateTime.UtcNow;
            ChildTags = new List<BaseTag>();
        }

        /// <summary>
        /// Allows you to specify the Value and Type of the object. E.g. BaseTag ageTag = new BaseTag("Age", 42, "Person's age"); Value = 42, Type = Int32
        /// </summary>
        private void SetValue(object value)
        {
            Value = value;
            Type = value?.GetType().Name;
        }
        /// <summary>
        /// Allows a collection of tags to be assigned to a parent tag.
        /// </summary>
        public void AddChildTag(BaseTag childTag)
        {
            if(childTag == null)
            {
                throw new ArgumentNullException(nameof(childTag));
            }         
            ChildTags.Add(childTag);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
