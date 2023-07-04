namespace ObjectMetaDataTagging.Models
{
    // A flexible mechanism for associating metadata with object instances.

    public class PersonTransaction
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public double Amount { get; set; }

        public override string ToString()
        {
            return $"Sender: {Sender}, Receiver: {Receiver}, Amound: {Amount}";
        }
    }


    public class CompanyTransaction
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public double Amount { get; set; }

        public override string ToString()
        {
            return $"Sender: {Sender}, Receiver: {Receiver}, Amound: {Amount}";
        }
    }

}