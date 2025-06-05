using Newtonsoft.Json;

namespace AppleNotificationV2Webhook.Models;

public class AppleSignedPayloadRequestDto
{
    [JsonProperty("signedPayload")]
    public string SignedPayload { get; set; }
}
