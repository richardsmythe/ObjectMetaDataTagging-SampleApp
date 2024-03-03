using System;

namespace ObjectMetaDataTagging.Models
{
    public class ExamplePersonTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public double Amount { get; set; }

        public override string ToString()
        {
            return $"Sender: {Sender}, Receiver: {Receiver}, Amount: {Amount}";
        }

        public override int GetHashCode()
        {
            // Combine the hash codes of the properties
            return HashCode.Combine(Id, Sender, Receiver, Amount);
        }

        public override bool Equals(object obj)
        {
            // Check if the object is null or not of the same type
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // Convert the object to ExamplePersonTransaction
            ExamplePersonTransaction other = (ExamplePersonTransaction)obj;

            // Check if all properties are equal
            return Id.Equals(other.Id) &&
                   Sender == other.Sender &&
                   Receiver == other.Receiver &&
                   Amount.Equals(other.Amount);
        }
    }
}
