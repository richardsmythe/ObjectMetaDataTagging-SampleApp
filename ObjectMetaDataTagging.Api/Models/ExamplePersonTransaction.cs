namespace ObjectMetaDataTagging.Models
{

    public class ExamplePersonTransaction
    {
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