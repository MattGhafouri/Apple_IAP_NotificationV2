namespace AppleNotificationV2Webhook.Models;


public class FinalAppleNotification
{
    public AppStoreServerNotificationV2? Notification { get; set; }
    public JWSTransactionDecodedPayload? Transaction { get; set; }
    public JWSRenewalInfoDecodedPayload? RenewalInfo { get; set; }
}
