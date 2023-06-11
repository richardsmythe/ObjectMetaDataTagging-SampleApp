namespace ObjectMetaDataTagging.Models
{
    // A flexible mechanism for associating metadata with object instances.

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, Age: {Age}";
        }
    }

}