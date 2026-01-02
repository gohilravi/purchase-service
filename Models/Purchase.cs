namespace purchase_service.Models;

public class Purchase
{
    public int Id { get; set; }
    public int OfferId { get; set; }
    public int BuyerId { get; set; }
    public int StatusTypeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }

    // Navigation properties
    public Buyer Buyer { get; set; } = null!;
    public StatusType StatusType { get; set; } = null!;
}
