using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Models
{

    public class ExamplePersonTransaction
    {
        public Guid Id { get; set; } = new Guid();
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public double Amount { get; set; }

        public List<BaseTag> AssociatedTags { get; } = new List<BaseTag>();
        public override string ToString()
        {
            return $"Sender: {Sender}, Receiver: {Receiver}, Amound: {Amount}";
        }
    }


}