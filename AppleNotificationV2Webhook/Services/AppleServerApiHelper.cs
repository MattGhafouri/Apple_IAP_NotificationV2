
using AppleNotificationV2Webhook.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AppleNotificationV2Webhook.Services;

public class AppleServerApiHelper
{

    public AppleServerApiHelper(
        ILogger<AppleServerApiHelper> logger)
    {
         
    }

    private const string AppleJwksUrl = "https://appleid.apple.com/auth/keys";
    private readonly string AppleTestSandboxUrl = "https://api.storekit-sandbox.itunes.apple.com/inApps/v1/notifications/test";
    private readonly string bundleId = "SetYourBundleId";
    private readonly string keyId = "YourKidId"; // Your Apple Key ID
    private readonly string issuer = "Issuer Id";
    private readonly string privateKeyPem = @"
                -----BEGIN PRIVATE KEY-----
               Your Private Key value
                -----END PRIVATE KEY-----
                ";
    private const string VerifyReceiptProductionUrl = "https://buy.itunes.apple.com/verifyReceipt";
    private const string VerifyReceiptSandboxUrl = "https://sandbox.itunes.apple.com/verifyReceipt";
    
    //path :https://appstoreconnect.apple.com/access/integrations/shared-secret
    private const string _sharedSecret = "yourSharedSecret";




    #region Test Notification

    public async Task SendTestNotificationV2Async()
    {

        var jwt = GenerateJwtToken();

        using var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, AppleTestSandboxUrl);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        request.Content = new StringContent(JsonSerializer.Serialize(new { bundleId = bundleId }));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

    }

    #endregion

    #region Validate Receipt


    //Status Code	Meaning
    //0	            Receipt is valid
    //21007	        Receipt is from Sandbox, send to sandbox URL
    //21010	        Could not parse receipt
    //21002	        Receipt data malformed
    //21003	        Receipt authentication failed
    //Other         Various errors, check Apple docs
    //Your iOS app sends the Base64 receipt to your backend after purchase or subscription restore
    //Your backend calls ValidateReceiptAsync() with the receipt string
    //Check Status == 0 → receipt is valid
    //Inspect LatestReceiptInfo or Receipt to check subscription expiry, product ID, etc.
    //Update your DB accordingly(e.g., mark subscription active/expired)
    public async Task<AppleReceiptResponse> ValidateReceiptAsync(string base64Receipt)
    {
        var requestBody = new
        {
            receipt_data = base64Receipt,
            password = _sharedSecret,
            exclude_old_transactions = true
        };

        var json = JsonSerializer.Serialize(requestBody);

        var response = await PostReceiptAsync(VerifyReceiptProductionUrl, json);

        if (response is null)
            throw new ApplicationException("ValidateReceipt failed. Respose is null");

        if (response.Status == 21007) // Sandbox receipt sent to production endpoint
        {
            // Retry sandbox environment
            response = await PostReceiptAsync(VerifyReceiptSandboxUrl, json);
        }
        else if (response.Status is 0)
        {
            //Receipt is valid

            //here we should update|create subscription for user
        }

        return response;
    }

    private async Task<AppleReceiptResponse?> PostReceiptAsync(string url, string json)
    {
        using var httpClient = new HttpClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var httpResponse = await httpClient.PostAsync(url, content);

        var responseString = await httpResponse.Content.ReadAsStringAsync();

        if (responseString is null)
            return null;

        return JsonSerializer.Deserialize<AppleReceiptResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    #endregion

    public string GenerateJwtToken()
    {
        var ecdsa = GetECDsaPrivateKey();
        var securityKey = new ECDsaSecurityKey(ecdsa) { KeyId = keyId };

        var header = new JwtHeader(new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256));
        header["kid"] = keyId;
        header["typ"] = "JWT";

        var now = DateTimeOffset.UtcNow;
        var iat = now.ToUnixTimeSeconds();
        var exp = now.AddMinutes(30).ToUnixTimeSeconds(); // 5 min expiration

        var payload = new JwtPayload
                    {
                        { "iss", issuer },
                        { "iat", iat },
                        { "exp", exp },
                        { "aud", "appstoreconnect-v1" },
                        { "bid", bundleId }
                    };

        var token = new JwtSecurityToken(header, payload);
        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(token);
    }

    private ECDsa GetECDsaPrivateKey()
    {
        // Remove PEM headers/footers and decode base64
        var key = privateKeyPem.Replace("-----BEGIN PRIVATE KEY-----", "")
                               .Replace("-----END PRIVATE KEY-----", "")
                               .Replace("\n", "")
                               .Replace("\r", "")
                               .Trim();
        var privateKeyBytes = Convert.FromBase64String(key);

        // Import the key into ECDsa
        var ecdsa = ECDsa.Create();
        ecdsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
        return ecdsa;
    }


    /// <summary>
    /// Newtonsoft.Json workd fine but Microsoft.Test does not serialzie proeprly
    /// </summary>
    public FinalAppleNotification DecodeNotificationJWT(string signedPayload)
    {
        var notificationJson = GetPayloadJson(signedPayload);

        if (string.IsNullOrEmpty(notificationJson))
            throw new ApplicationException("singedPayload is null");

        var notification = Newtonsoft.Json.JsonConvert.DeserializeObject<AppStoreServerNotificationV2>(notificationJson);

        if (notification is null)
            throw new ApplicationException("deserialzied notification is null");


        var singedTrx = GetPayloadJson(notification.Data.SignedTransactionInfo);
        JWSTransactionDecodedPayload? transactionDecoded = null;
        if (!string.IsNullOrEmpty(singedTrx))
            transactionDecoded = Newtonsoft.Json.JsonConvert.DeserializeObject<JWSTransactionDecodedPayload>(singedTrx);

        JWSRenewalInfoDecodedPayload? renewalInfoDecodedPayload = null;
        var singedRenewalInfo = GetPayloadJson(notification.Data.SignedRenewalInfo);
        if (!string.IsNullOrEmpty(singedRenewalInfo))
            renewalInfoDecodedPayload = Newtonsoft.Json.JsonConvert.DeserializeObject<JWSRenewalInfoDecodedPayload>(singedRenewalInfo);

        //now we need to case the rest here
        return new FinalAppleNotification
        {
            Notification = notification,
            Transaction = transactionDecoded,
            RenewalInfo = renewalInfoDecodedPayload
        };

    }

    private string? GetPayloadJson(string signedPayload)
    {
        var parts = signedPayload.Split('.');
        if (parts.Length != 3)
            return null;

        var header = parts[0];
        var payload = parts[1];

        // Add padding if necessary
        header = header.PadRight(header.Length + (4 - header.Length % 4) % 4, '=');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

        //header
        var jsonBytes_header = Convert.FromBase64String(header);
        var json_header = Encoding.UTF8.GetString(jsonBytes_header);


        //payload
        var jsonBytes = Convert.FromBase64String(payload);
        var json = Encoding.UTF8.GetString(jsonBytes);

        return json;
    }

    public string SerializeToJson(JwtPayload payload)
    {
        var dict = payload.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        return JsonSerializer.Serialize(dict);
    }



}
