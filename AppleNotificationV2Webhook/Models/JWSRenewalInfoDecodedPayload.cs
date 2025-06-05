using Newtonsoft.Json;

namespace AppleNotificationV2Webhook.Models;

/// <summary>
/// A decoded payload containing subscription renewal information for an auto-renewable subscription.
/// </summary>

public class JWSRenewalInfoDecodedPayload
{
    [JsonProperty("originalTransactionId")]
    public string OriginalTransactionId { get; set; }

    [JsonProperty("autoRenewProductId")]
    public string AutoRenewProductId { get; set; }

    [JsonProperty("productId")]
    public string ProductId { get; set; }

    [JsonProperty("autoRenewStatus")]
    public int AutoRenewStatus { get; set; }

    [JsonProperty("renewalPrice")]
    public int RenewalPrice { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; }

    [JsonProperty("signedDate")]
    public long SignedDate { get; set; }

    [JsonProperty("environment")]
    public string Environment { get; set; }

    [JsonProperty("recentSubscriptionStartDate")]
    public long RecentSubscriptionStartDate { get; set; }

    [JsonProperty("renewalDate")]
    public long RenewalDate { get; set; }

    [JsonProperty("appTransactionId")]
    public string AppTransactionId { get; set; }
}
