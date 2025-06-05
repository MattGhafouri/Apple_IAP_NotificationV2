namespace AppleNotificationV2Webhook.Models;

public class LatestReceiptInfo
{
    public string ProductId { get; set; }
    public string TransactionId { get; set; }
    public string OriginalTransactionId { get; set; }
    public string PurchaseDate { get; set; }
    public string ExpiresDate { get; set; }
    // Other fields as needed
}
