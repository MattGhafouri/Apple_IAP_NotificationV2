using AppleNotificationV2Webhook.Models;
using Newtonsoft.Json;


namespace AppleNotificationV2Webhook.Services;

public   class AppleWebhookHandlerAppService 
{
    private readonly AppleServerApiHelper _appleServerApiHelper;
    private readonly ILogger<AppleWebhookHandlerAppService> _logger;
    public AppleWebhookHandlerAppService(
        AppleServerApiHelper appleServerApiHelper,
        ILogger<AppleWebhookHandlerAppService> logger)
    {
        _appleServerApiHelper = appleServerApiHelper;
        _logger = logger;
    }



    public async Task SendTestNotificationV2Async()
    {
        await _appleServerApiHelper.SendTestNotificationV2Async();
    }


    /// <summary>
    /// espond to the App Store with an HTTP status code of 200-206 if the post was successful. 
    /// If the post was unsuccessful, send HTTP 50x or 40x to have the App Store retry the notification.
    /// </summary>
    public async Task HandleNotificationAsync(string signedPayload)
    {
        var correlationId = Guid.NewGuid();
        try
        {
            _logger.LogInformation(1000,
                $"{correlationId} . iap-notification received. payload : {JsonConvert.SerializeObject(signedPayload)}");


            if (string.IsNullOrEmpty(signedPayload))
                throw new ApplicationException($"{correlationId} .Missing signed Payload");


            var finalAppleNotification = _appleServerApiHelper.DecodeNotificationJWT(signedPayload);


            // Process notification
            await ProcessNotificationV2Async(correlationId, finalAppleNotification);

            _logger.LogInformation(1000,
                $"{correlationId} . iap-notification processed.");

        }
        catch (Exception ex)
        {
            _logger.LogError(1000, ex,
                $"HandleEventAsync failed. correlationId : {correlationId}", true);
        }
    }


    private async Task ProcessNotificationV2Async(Guid correlationId, FinalAppleNotification notificationV2)
    {
        _logger.LogInformation(
            $"{correlationId} . ProcessNotificationAsync started . " +
            $"\nNotification Type: {notificationV2?.Notification?.NotificationType}" +
            $"\nNotification SubType: {notificationV2?.Notification?.Subtype}" +
            $"\nNotification Environment: {notificationV2?.Notification?.Data?.Environment}" +
            $"\nNotification Status: {notificationV2?.Notification?.Data?.Status}" +
            $"\nTransaction Data: {JsonConvert.SerializeObject(notificationV2?.Transaction)}" +
            $"\nRenewalInfo Data: {JsonConvert.SerializeObject(notificationV2?.RenewalInfo)}");


        await HandleNotificationBasedOnType(correlationId, notificationV2);
    }

    private async Task HandleNotificationBasedOnType
        (Guid correlationId, FinalAppleNotification notificationDetail)
    {
        if (!Enum.TryParse<AppleNotificationType>(notificationDetail.Notification.NotificationType, ignoreCase: true, out var typeEnum))
            throw new ApplicationException($"Unknown apple notification." +
                $" correlationId:{correlationId} . type: {notificationDetail.Notification.NotificationType}," +
                $" subType:{notificationDetail.Notification.Subtype}");

        Enum.TryParse<AppleNotificationSubtype>(notificationDetail.Notification.Subtype, ignoreCase: true, out var subtypeEnum);


        switch (typeEnum)
        {
            case AppleNotificationType.ONE_TIME_CHARGE:
                await HandleOneTimeChargeAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.SUBSCRIBED:
                switch (subtypeEnum)
                {
                    case AppleNotificationSubtype.INITIAL_BUY:
                        await HandleInitialBuyAsync(correlationId, notificationDetail);
                        break;
                    case AppleNotificationSubtype.RESUBSCRIBE:
                        await HandleResubscribeAsync(correlationId, notificationDetail);
                        break;
                }
                break;

            case AppleNotificationType.DID_RENEW:
                if (subtypeEnum == AppleNotificationSubtype.BILLING_RECOVERY)
                    await HandleBillingRecoveryAsync(correlationId, notificationDetail);
                else
                    await HandleRenewalAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.DID_CHANGE_RENEWAL_PREF:
                if (subtypeEnum == AppleNotificationSubtype.UPGRADE)
                    await HandleUpgradeAsync(correlationId, notificationDetail);
                else if (subtypeEnum == AppleNotificationSubtype.DOWNGRADE)
                    await HandleDowngradeAsync(correlationId, notificationDetail);
                else
                    await HandleRenewalPrefChangeAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.DID_CHANGE_RENEWAL_STATUS:
                if (subtypeEnum == AppleNotificationSubtype.AUTO_RENEW_DISABLED)
                    await HandleAutoRenewDisabledAsync(correlationId, notificationDetail);
                else if (subtypeEnum == AppleNotificationSubtype.AUTO_RENEW_ENABLED)
                    await HandleAutoRenewEnabledAsync(correlationId, notificationDetail);
                else
                    await HandleRenewalStatusChangeAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.EXPIRED:
                switch (subtypeEnum)
                {
                    case AppleNotificationSubtype.VOLUNTARY:
                        await HandleExpiredVoluntaryAsync(correlationId, notificationDetail);
                        break;
                    case AppleNotificationSubtype.PRODUCT_NOT_FOR_SALE:
                        await HandleExpiredProductNotForSaleAsync(correlationId, notificationDetail);
                        break;
                    case AppleNotificationSubtype.BILLING_RETRY:
                        await HandleExpiredBillingRetryAsync(correlationId, notificationDetail);
                        break;
                    case AppleNotificationSubtype.PRICE_INCREASE:
                        await HandleExpiredPriceIncreaseAsync(correlationId, notificationDetail);
                        break;
                    default:
                        await HandleExpiredUnknownAsync(correlationId, notificationDetail);
                        break;
                }
                break;

            case AppleNotificationType.DID_FAIL_TO_RENEW:
                if (subtypeEnum == AppleNotificationSubtype.GRACE_PERIOD)
                    await HandleGracePeriodAsync(correlationId, notificationDetail);
                else
                    await HandleFailedToRenewAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.GRACE_PERIOD_EXPIRED:
                await HandleGracePeriodExpiredAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.PRICE_INCREASE:
                if (subtypeEnum == AppleNotificationSubtype.PENDING)
                    await HandlePriceIncreasePendingAsync(correlationId, notificationDetail);
                else if (subtypeEnum == AppleNotificationSubtype.ACCEPTED)
                    await HandlePriceIncreaseAcceptedAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.OFFER_REDEEMED:
                if (subtypeEnum == AppleNotificationSubtype.UPGRADE)
                    await HandleOfferUpgradeAsync(correlationId, notificationDetail);
                else if (subtypeEnum == AppleNotificationSubtype.DOWNGRADE)
                    await HandleOfferDowngradeAsync(correlationId, notificationDetail);
                else
                    await HandleGenericOfferRedeemedAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.REFUND:
                await HandleRefundAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.REFUND_REVERSED:
                await HandleRefundReversedAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.REFUND_DECLINED:
                await HandleRefundDeclinedAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.CONSUMPTION_REQUEST:
                await HandleConsumptionRequestAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.REVOKE:
                await HandleFamilySharingRevokedAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.RENEWAL_EXTENDED:
                if (subtypeEnum == AppleNotificationSubtype.FAILURE)
                    await HandleRenewalExtensionFailureAsync(correlationId, notificationDetail);
                else
                    await HandleRenewalExtendedAsync(correlationId, notificationDetail);
                break;

            case AppleNotificationType.RENEWAL_EXTENSION:
                if (subtypeEnum == AppleNotificationSubtype.SUMMARY)
                    await HandleRenewalExtensionSummaryAsync(correlationId, notificationDetail);
                else if (subtypeEnum == AppleNotificationSubtype.FAILURE)
                    await HandleRenewalExtensionFailureAsync(correlationId, notificationDetail);
                break;

            default:
                _logger.LogWarning(
                    $"Unhandled notification type: {typeEnum} with subtype: {subtypeEnum}. correlationId:{correlationId}");
                break;
        }
    }


    #region notification handlers

    #region Required Events


    /// <summary>
    /// not listed in the documentation,
    /// </summary>
    private async Task HandleExpiredUnknownAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //we need to cancel subscription for user here 

        //and log the detail to see the unknown reason

        UpdateSubscription(correlationId, notificationDetail);
    }

    /// <summary>
    /// The auto-renewable subscription expires because the customer didn’t consent to the price increase that requires consent.
    /// </summary>
    private async Task HandleExpiredPriceIncreaseAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //we need to cancel subscription for user here 

        //maybe for marketing purposes-sending promo code maybe

        UpdateSubscription(correlationId, notificationDetail);
    }

    /// <summary>
    /// The subscription expires because the billing retry period ends without recovering the subscription.
    /// </summary>
    private async Task HandleExpiredBillingRetryAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //we need to cancel subscription for user here 

        //maybe for marketing purposes

        UpdateSubscription(correlationId, notificationDetail);
    }

    /// <summary>
    /// The subscription expires because the developer removed the subscription from sale and the renewal fails.
    /// </summary>
    private async Task HandleExpiredProductNotForSaleAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //we need to cancel subscription for user here 

        //maybe for marketing purposes

        UpdateSubscription(correlationId, notificationDetail);
    }

    /// <summary>
    /// The subscription expires because the customer chose to cancel it.
    /// </summary>
    private async Task HandleExpiredVoluntaryAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //we need to cancel subscription for user here 

        //maybe for marketing purposes


        //1- read subscription detail from api
        // 2- fetch subscription data from db
        // 3-  compare and in case required update subscription (start,end, status)
        UpdateSubscription(correlationId, notificationDetail);

    }


    /// <summary>
    /// Customer downgrades a subscription within the same subscription group.
    /// </summary>
    private async Task HandleDowngradeAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //we need to update subscription for user here 
        //maybe for marketing purposes
        UpdateSubscription(correlationId, notificationDetail);
    }


    /// <summary>
    /// Customer upgrades a subscription within the same subscription group.
    /// </summary>
    private async Task HandleUpgradeAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //we need to update subscription for user here 
        //maybe for marketing purposes
        UpdateSubscription(correlationId, notificationDetail);
    }


    /// <summary>
    /// Customer resubscribes to any subscription from the same subscription group as their expired subscription.
    /// </summary>
    private async Task HandleResubscribeAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //we need to update subscription for user here 
        //maybe for marketing purposes -sending email
        UpdateSubscription(correlationId, notificationDetail);
    }

    /// <summary>
    /// Customer subscribes for the first time to any subscription within a subscription group.
    /// </summary>
    private async Task HandleInitialBuyAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //we need to create subscription for user here if not exist

        //maybe for marketing purposes

        UpdateSubscription(correlationId, notificationDetail);
    }



    private void UpdateSubscription(Guid correlationId, FinalAppleNotification notificationDetail)
    {

        var startDate = notificationDetail?.Transaction?.PurchaseDate is null ? DateTime.UtcNow.AddDays(-1000)
            : DateTimeOffset.FromUnixTimeMilliseconds(notificationDetail.Transaction.PurchaseDate.Value).UtcDateTime;

        var endDate = notificationDetail?.Transaction?.ExpiresDate is null
             ? DateTime.UtcNow.AddDays(-1000)
        : DateTimeOffset.FromUnixTimeMilliseconds(notificationDetail.Transaction.ExpiresDate.Value).UtcDateTime;

        _logger.LogInformation($"Subscription Updated: " +
            $"\n correlationId: {correlationId}" +
            $"\nType:{notificationDetail?.Notification?.NotificationType} . Sub:{notificationDetail?.Notification?.Subtype}" +
            $"\nTransactionId:{notificationDetail?.Transaction?.TransactionId} . Start:{startDate}  .  EndDate:{endDate}");
    }
    #endregion

    #region Not Required Events
    /// <summary>
    /// The App Store successfully completes extending the subscription renewal date for all eligible subscribers.
    /// </summary>
    private async Task HandleRenewalExtensionSummaryAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //no need to do anything for this notification
    }

    /// <summary>
    /// The App Store successfully extends a subscription renewal date for a specific subscription.
    /// </summary>
    private async Task HandleRenewalExtendedAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //no need to do anything for this notification
    }

    /// <summary>
    /// The App Store failed to extend the subscription renewal date for a specific subscriber.
    /// </summary>
    private async Task HandleRenewalExtensionFailureAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //no need to do anything for this notification
    }

    /// <summary>
    /// A family member loses access to the subscription through Family Sharing.
    /// </summary>
    private async Task HandleFamilySharingRevokedAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //no need to do anything for this notification
    }

    /// <summary>
    /// Apple requests consumption information for a refund request that a customer initiates.
    /// </summary>
    private async Task HandleConsumptionRequestAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        // we need to check how we cna return this data to apple.
        //this notification seems we will need it to handle refund scenarios
    }

    /// <summary>
    /// Apple declines a refund that the customer initiated in the app, using the request refund API.
    /// </summary>
    private async Task HandleRefundDeclinedAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        // we need to log this specific event to monitor the nubmer fo refund requested
    }

    /// <summary>
    /// Apple reverses a previously granted refund due to a dispute that the customer raised.
    /// </summary>
    private async Task HandleRefundReversedAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        // we need to log this specific event to monitor the nubmer fo refund requested
    }

    /// <summary>
    /// Apple refunds the transaction for a consumable or non-consumable in-app purchase, 
    /// a non-renewing subscription, or an auto-renewable subscription.
    /// </summary>
    private async Task HandleRefundAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        // we need to log this specific event to monitor the nubmer fo refund requested,but in the apple dasbhoad we will have this data though
    }

    /// <summary>
    /// Customer redeems a promotional offer or offer code for an active subscription.
    /// </summary>
    private async Task HandleGenericOfferRedeemedAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //just log this notification to track in case someone tries to cheat with promocode
    }

    /// <summary>
    /// Customer redeems a promotional offer and downgrades their subscription.
    /// </summary>
    private async Task HandleOfferDowngradeAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //just log this notification to track in case someone tries to cheat with promocode
    }

    /// <summary>
    /// Customer redeems a promotional offer or offer code to upgrade their subscription.
    /// </summary>
    private async Task HandleOfferUpgradeAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //just log this notification to track in case someone tries to cheat with promocode
    }

    /// <summary>
    /// Customer consents to an auto-renewable subscription price increase that requires consent.
    /// </summary>
    private async Task HandlePriceIncreaseAcceptedAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //no need to do anything here
    }

    /// <summary>
    /// The system informs the customer of the auto-renewable subscription price increase that requires customer consent, and the customer doesn’t respond.
    /// </summary>
    private async Task HandlePriceIncreasePendingAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //no need to do anything here
    }




    /// <summary>
    /// Customer subscribes again after canceling a subscription, which reenables auto-renew.
    /// </summary>
    private async Task HandleAutoRenewEnabledAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //just log this notification we do nto need it for now
        //maybe for marketing purposes
    }

    /// <summary>
    /// Customer cancels the subscription from the App Store Subscriptions settings page.
    /// or
    /// The system disabled auto-renew because the customer initiated a refund through your app using the refund request API.
    /// </summary>
    private async Task HandleAutoRenewDisabledAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //just log this notification we do nto need it for now
        //maybe for marketing purposes
    }

    /// <summary>
    /// Customer reverts to the previous subscription, effectively canceling their downgrade.
    /// </summary>
    private async Task HandleRenewalPrefChangeAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //just log this notification we do nto need it for now

    }

    /// <summary>
    /// The subscription exits the billing grace period (and continues in billing retry).
    /// </summary>
    private async Task HandleGracePeriodExpiredAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //no need to do anything here
    }

    /// <summary>
    /// The subscription fails to renew and enters the billing retry period.
    /// </summary>
    private async Task HandleFailedToRenewAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //no need to do anything here

        //maybe for marketing purposes
    }

    /// <summary>
    /// The subscription fails to renew and enters the billing retry period with Billing Grace Period enabled.
    /// </summary>
    private async Task HandleGracePeriodAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //no need to do anything here
    }
    /// <summary>
    /// Customer canceled the subscription after receiving a price increase notice or a request to consent to a price increase.
    /// </summary>
    private async Task HandleRenewalStatusChangeAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //maybe for marketing purposes - maybe sending a promo code to user
    }



    /// <summary>
    /// Customer purchases a consumable, non-consumable, or non-renewing subscription.
    /// </summary>
    private async Task HandleOneTimeChargeAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //just log this notification we do nto need it for now
    }


    /// <summary>
    /// The subscription successfully auto-renews.
    /// </summary>
    private async Task HandleRenewalAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //maybe for marketing purposes -sending email
    }

    /// <summary>
    /// The billing retry successfully recovers the subscription.
    /// </summary>
    private async Task HandleBillingRecoveryAsync(Guid correlationId, FinalAppleNotification notificationDetail)
    {
        //maybe for marketing purposes -sending email
    }

    #endregion



    #endregion
}
