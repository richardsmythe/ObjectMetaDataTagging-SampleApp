﻿using System;

namespace ObjectMetaDataTagging.Models
{
    /// <summary>
    /// A base tag providing basic tag object properties that potential derived classes
    /// may require.
    /// </summary>
    public class BaseTag
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }
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
        private void SetValue(object value)
        {
            Value = value;
            Type = value?.GetType().Name;
        }



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



        public override string ToString()
        {
            return Name;
        }
    }
}
