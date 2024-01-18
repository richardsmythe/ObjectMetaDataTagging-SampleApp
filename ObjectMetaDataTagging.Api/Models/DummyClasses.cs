using ObjectMetaDataTagging.Models.TagModels;

public class DummyBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Sender { get; set; }
    public string Receiver { get; set; }
    public double Amount { get; set; }

    public List<BaseTag> AssociatedTags { get; } = new List<BaseTag>();

    public override string ToString()
    {
        return $"Sender: {Sender}, Receiver: {Receiver}, Amount: {Amount}";
    }
}

public class Transaction : DummyBase { }
public class Fraud : DummyBase { }
public class Address : DummyBase { }