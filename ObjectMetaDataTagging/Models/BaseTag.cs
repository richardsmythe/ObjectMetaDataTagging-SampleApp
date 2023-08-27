using System;

namespace ObjectMetaDataTagging.Models
{
    public class BaseTag
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public DateTime DateCreated { get; private set; }
        public DateTime? DateLastUpdated { get; private set; }  
        public string Description { get; set; }
        public object Value { get; set; }
        public Guid AssociatedObjectId { get; set; }
        public string Type { get; private set; } 

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
        }

        /// <summary>
        /// Updates the tag's name, description, and value.
        /// </summary>
        //public void UpdateTag(string newName, string newDescription, object newValue)
        //{
        //    if (string.IsNullOrWhiteSpace(newName))
        //    {
        //        throw new ArgumentNullException(nameof(newName));
        //    }

        //    Name = newName;
        //    Description = newDescription;
        //    SetValue(newValue);
        //    DateLastUpdated = DateTime.UtcNow;
        //}

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
