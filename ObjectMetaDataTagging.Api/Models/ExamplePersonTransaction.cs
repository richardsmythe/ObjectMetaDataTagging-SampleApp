using ObjectMetaDataTagging.Models.TagModels;

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
    }

}