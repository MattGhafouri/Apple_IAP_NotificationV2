namespace AppleNotificationV2Webhook.Models;

//https://developer.apple.com/documentation/appstoreservernotifications/subtype
public enum AppleNotificationSubtype
{
    INITIAL_BUY,
    RESUBSCRIBE,
    BILLING_RECOVERY,
    UPGRADE,
    DOWNGRADE,
    AUTO_RENEW_DISABLED,
    AUTO_RENEW_ENABLED,
    VOLUNTARY,
    PRODUCT_NOT_FOR_SALE,
    BILLING_RETRY,
    PRICE_INCREASE,
    GRACE_PERIOD,
    PENDING,
    ACCEPTED,
    FAILURE,
    SUMMARY,
    UNKNOWN // fallback for unmatched subtypes
}
