using AppleNotificationV2Webhook.Models;
using AppleNotificationV2Webhook.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppleNotificationV2Webhook;

/// <summary>
/// Just Take it into account that there will be udpates for request structure or APIs. you may need to read IAP apple document to 
/// make sure there is nothing outdated here
/// </summary>
[AllowAnonymous]
[Route("applePayment")]
public class AppleWebhookController : ControllerBase
{
    private readonly AppleWebhookHandlerAppService _webhookAppService;

    public AppleWebhookController(AppleWebhookHandlerAppService service)
    {
        _webhookAppService = service;
    }
    /// <summary>
    /// This is the webhook endpoint that is called by apple sever
    /// check App Store Server Notifications in appstoreconnect
    [HttpPost("notification")]
    public async Task<IActionResult> ApplePayNotificationV2([FromBody] AppleSignedPayloadRequestDto request)
    {
        await _webhookAppService.HandleNotificationAsync(request.SignedPayload);
        return Ok();
    }



    [HttpPost("TestIAP")]
    
    public async Task<IActionResult> SendTestNotificationV2()
    {
        await _webhookAppService.SendTestNotificationV2Async();
        return Ok();
    }
} 
 



