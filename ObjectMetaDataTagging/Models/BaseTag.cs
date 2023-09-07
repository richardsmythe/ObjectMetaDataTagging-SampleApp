using System;

namespace ObjectMetaDataTagging.Models
{
    /// <summary>
    /// A base tag providing basic tag object properties that potential derived classes
    /// may require.
    /// </summary>
    public class BaseTag
    {
        public Guid Id { get; private set; }
        public Guid? ParentTagId { get; private set; } // for tag hierarchies - not implemented
        public string Name { get; set; }
        public DateTime DateCreated { get; private set; }
        public DateTime? DateLastUpdated { get; set; }  
        public string Description { get; set; }
        public object Value { get; set; }
        public object AssociatedParentObjectName { get; set; }
        public Guid AssociatedParentObjectId { get; set; }
        public string Type { get; private set; }

        public BaseTag(string name, object value, string description = null, Guid? parentTagId = null)
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
            ParentTagId = parentTagId;
        }
        private void SetValue(object value)
        {
            Value = value;
            Type = value?.GetType().Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
