using Newtonsoft.Json;

namespace AppleNotificationV2Webhook.Models;

public class AppStoreServerNotificationV2
{
    [JsonProperty("notificationType")]
    public string NotificationType { get; set; }

    [JsonProperty("subtype")]
    public string Subtype { get; set; }

    [JsonProperty("data")]
    public AppStoreNotificationData Data { get; set; }


    /// <summary>
    /// For now we do not need these properties, although they are received but , they won't prsed since we do not need them
    /// </summary>
    /*
    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("signedDate")]
    public long SignedDate { get; set; }

    [JsonProperty("notificationUUID")]
    public string NotificationUUID { get; set; }

    [JsonProperty("summary")]
    public AppStoreNotificationSummary Summary { get; set; }

    [JsonProperty("externalPurchaseToken")]
    public AppStoreExternalPurchaseToken ExternalPurchaseToken { get; set; }

    */
}
