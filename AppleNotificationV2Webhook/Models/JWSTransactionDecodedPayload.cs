using Newtonsoft.Json;

namespace AppleNotificationV2Webhook.Models;

/// <summary>
/// A decoded payload that contains transaction information from the App Store Server API.
/// </summary>
public class JWSTransactionDecodedPayload
{
    [JsonProperty("appAccountToken")]
    public string? AppAccountToken { get; set; }

    [JsonProperty("appTransactionId")]
    public string? AppTransactionId { get; set; }

    [JsonProperty("bundleId")]
    public string? BundleId { get; set; }

    [JsonProperty("currency")]
    public string? Currency { get; set; }

    [JsonProperty("environment")]
    public string? Environment { get; set; }

    [JsonProperty("expiresDate")]
    public long? ExpiresDate { get; set; }

    [JsonProperty("inAppOwnershipType")]
    public string? InAppOwnershipType { get; set; }

    [JsonProperty("isUpgraded")]
    public bool? IsUpgraded { get; set; }

    [JsonProperty("offerDiscountType")]
    public int? OfferDiscountType { get; set; }

    [JsonProperty("offerIdentifier")]
    public string? OfferIdentifier { get; set; }

    [JsonProperty("offerPeriod")]
    public string? OfferPeriod { get; set; }

    [JsonProperty("offerType")]
    public int? OfferType { get; set; }

    [JsonProperty("originalPurchaseDate")]
    public long? OriginalPurchaseDate { get; set; }

    [JsonProperty("originalTransactionId")]
    public string? OriginalTransactionId { get; set; }

    [JsonProperty("price")]
    public long? Price { get; set; }

    [JsonProperty("productId")]
    public string? ProductId { get; set; }

    [JsonProperty("purchaseDate")]
    public long? PurchaseDate { get; set; }

    [JsonProperty("quantity")]
    public int? Quantity { get; set; }

    [JsonProperty("revocationDate")]
    public long? RevocationDate { get; set; }

    [JsonProperty("revocationReason")]
    public int? RevocationReason { get; set; }

    [JsonProperty("storefront")]
    public string? Storefront { get; set; }

    [JsonProperty("storefrontId")]
    public string? StorefrontId { get; set; }

    [JsonProperty("subscriptionGroupIdentifier")]
    public string? SubscriptionGroupIdentifier { get; set; }

    [JsonProperty("transactionId")]
    public string? TransactionId { get; set; }

    [JsonProperty("transactionReason")]
    public string? TransactionReason { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("webOrderLineItemId")]
    public string? WebOrderLineItemId { get; set; }

    [JsonProperty("advancedCommerceInfo")]
    public object? AdvancedCommerceInfo { get; set; }
}
