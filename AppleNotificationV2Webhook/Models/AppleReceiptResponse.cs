namespace AppleNotificationV2Webhook.Models;

public class AppleReceiptResponse
{
    public int Status { get; set; }
    public Receipt Receipt { get; set; }
    public LatestReceiptInfo[] LatestReceiptInfo { get; set; }
    public string LatestReceipt { get; set; }
    // Other fields as needed
}
