using Newtonsoft.Json;

namespace AppleNotificationV2Webhook.Models;

public class AppStoreNotificationData
{
    [JsonProperty("appAppleId")]
    public long? AppAppleId { get; set; }

    [JsonProperty("bundleId")]
    public string BundleId { get; set; }

    [JsonProperty("bundleVersion")]
    public string BundleVersion { get; set; }

    [JsonProperty("consumptionRequestReason")]
    public string ConsumptionRequestReason { get; set; }

    [JsonProperty("environment")]
    public string Environment { get; set; }

    [JsonProperty("signedRenewalInfo")]
    public string SignedRenewalInfo { get; set; }

    [JsonProperty("signedTransactionInfo")]
    public string SignedTransactionInfo { get; set; }

    [JsonProperty("status")]
    public int? Status { get; set; }
}
