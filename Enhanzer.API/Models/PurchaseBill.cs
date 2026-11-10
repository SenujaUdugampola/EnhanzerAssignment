namespace Enhanzer.API.Models;

public class PurchaseBill
{
    public long Id { get; set; }
    public string BillNo { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<PurchaseBillItem> Items { get; set; } = [];
}

public class PurchaseBillItem
{
    public long Id { get; set; }
    public long PurchaseBillId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public decimal StandardCost { get; set; }
    public decimal StandardPrice { get; set; }
    public decimal MarginPercent { get; set; }
    public decimal Quantity { get; set; }
    public decimal FreeQuantity { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TotalCost { get; private set; }
    public decimal TotalSelling { get; private set; }
    public PurchaseBill PurchaseBill { get; set; } = null!;
}

public sealed record CreatePurchaseBillRequest(IReadOnlyCollection<CreatePurchaseBillItemRequest> Items);

public sealed record CreatePurchaseBillItemRequest(
    string ItemName,
    string BatchCode,
    decimal StandardCost,
    decimal StandardPrice,
    decimal MarginPercent,
    decimal Quantity,
    decimal FreeQuantity,
    decimal DiscountPercent);
