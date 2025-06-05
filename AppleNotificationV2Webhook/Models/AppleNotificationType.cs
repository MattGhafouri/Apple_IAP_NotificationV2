namespace AppleNotificationV2Webhook.Models;

//https://developer.apple.com/documentation/appstoreservernotifications/notificationtype
public enum AppleNotificationType
{
    ONE_TIME_CHARGE,
    SUBSCRIBED,
    DID_RENEW,
    DID_CHANGE_RENEWAL_PREF,
    DID_CHANGE_RENEWAL_STATUS,
    EXPIRED,
    DID_FAIL_TO_RENEW,
    GRACE_PERIOD_EXPIRED,
    PRICE_INCREASE,
    OFFER_REDEEMED,
    REFUND,
    REFUND_REVERSED,
    REFUND_DECLINED,
    CONSUMPTION_REQUEST,
    REVOKE,
    RENEWAL_EXTENDED,
    RENEWAL_EXTENSION
}
