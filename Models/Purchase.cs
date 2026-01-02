using System;
using System.Collections.Generic;

namespace purchase.Models;

public partial class Purchase
{
    public int Id { get; set; }

    public int BuyerId { get; set; }

    public int OfferId { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public decimal Amount { get; set; }

    public string? BuyerInfo { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastModifiedAt { get; set; }
}
