namespace AppleNotificationV2Webhook.Models;

public class Receipt
{
    public string BundleId { get; set; }
    public string AppVersion { get; set; }
    public string OriginalPurchaseDate { get; set; }
    // More fields you want to parse
}
