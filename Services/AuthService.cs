using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace Fin_sCore.Services;

public class AuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiBaseUrl;
    private readonly IConfiguration _configuration;
    
    // Azure API URLs
    private readonly string _kvkkBaseUrl;
    private readonly string _otpBaseUrl;
    private readonly string _customerBaseUrl;

    public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        
        // WebSube-API Next.js proxy base URL from configuration (kept for backward compatibility)
        _apiBaseUrl = configuration["WebSubeApi:BaseUrl"] ?? "http://localhost:3000/api/ep";
        
        // Azure API Base URLs
        _kvkkBaseUrl = configuration["AzureApis:KvkkApi:BaseUrl"] ?? "https://api-idc.azurewebsites.net/api";
        _otpBaseUrl = configuration["AzureApis:OtpApi:BaseUrl"] ?? "https://api-idc.azurewebsites.net/api";
        _customerBaseUrl = configuration["AzureApis:CustomerApi:BaseUrl"] ?? "https://customers-api.azurewebsites.net/api";
    }

    #region Response Classes

    public class TcknGsmResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public TcknGsmValue? Value { get; set; }
    }

    public class TcknGsmValue
    {
        public int CustomerId { get; set; }
        public string TCKN { get; set; } = string.Empty;
        public string GSM { get; set; } = string.Empty;
    }

    public class SendOtpResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? OtpCode { get; set; }
        public int OtpId { get; set; }
        public int SmsVerificationId { get; set; }
    }

    public class VerifyOtpResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Value { get; set; }
    }

    public class IssueTokenResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public DateTime Expiry { get; set; }
    }

    public class KvkkTextResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class KvkkOnayResponse
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    #endregion

    #region Helper Methods

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Parses WebSube-API wrapper response and extracts inner data
    /// Response format: { status: number, data: T | string, elapsed: number, error?: string }
    /// Note: When API returns error, data might be a JSON string instead of object
    /// </summary>
    private static (bool success, JsonElement? data, string? error) ParseApiResponse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Check for error field
            if (root.TryGetProperty("error", out var errorProp) && errorProp.ValueKind == JsonValueKind.String)
            {
                var errorMsg = errorProp.GetString();
                if (!string.IsNullOrEmpty(errorMsg) && errorMsg != "null")
                {
                    // Try to get error message from data if it's a string containing JSON
                    if (root.TryGetProperty("data", out var dataForError))
                    {
                        var parsedError = TryParseDataField(dataForError);
                        if (parsedError.HasValue && parsedError.Value.TryGetProperty("message", out var msgProp))
                        {
                            return (false, null, msgProp.GetString() ?? errorMsg);
                        }
                    }
                    return (false, null, errorMsg);
                }
            }

            // Get status
            var status = 200;
            if (root.TryGetProperty("status", out var statusProp))
            {
                status = statusProp.GetInt32();
            }

            // Get data - might be object or string (double-encoded JSON)
            if (root.TryGetProperty("data", out var dataProp))
            {
                var parsedData = TryParseDataField(dataProp);
                if (parsedData.HasValue)
                {
                    // Check if parsed data indicates failure
                    if (parsedData.Value.TryGetProperty("success", out var successProp) && !successProp.GetBoolean())
                    {
                        var errorMessage = parsedData.Value.TryGetProperty("message", out var msgProp) 
                            ? msgProp.GetString() ?? "API hatası" 
                            : "API hatası";
                        return (false, parsedData, errorMessage);
                    }
                    return (status >= 200 && status < 300, parsedData, null);
                }
            }

            return (false, null, "No data in response");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// Tries to parse data field which might be an object or a JSON string
    /// </summary>
    private static JsonElement? TryParseDataField(JsonElement dataProp)
    {
        try
        {
            if (dataProp.ValueKind == JsonValueKind.Object)
            {
                return dataProp.Clone();
            }
            else if (dataProp.ValueKind == JsonValueKind.String)
            {
                // Data is a string containing JSON - parse it
                var dataString = dataProp.GetString();
                if (!string.IsNullOrEmpty(dataString))
                {
                    using var innerDoc = JsonDocument.Parse(dataString);
                    return innerDoc.RootElement.Clone();
                }
            }
        }
        catch
        {
            // Ignore parsing errors
        }
        return null;
    }

    #endregion

    #region TCKN-GSM Validation

    public async Task<TcknGsmResponse> ValidateTcknGsm(string tckn, string gsm)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            
            // Use WebSube-API proxy (Next.js) instead of direct Azure call
            var apiUrl = $"{_apiBaseUrl}/tckn-gsm";
            
            // GSM formatını düzelt (05 ile başlamalı)
            if (!gsm.StartsWith("0"))
            {
                gsm = "0" + gsm;
            }

            var requestData = new { tckn, gsm };
            var jsonContent = JsonSerializer.Serialize(requestData);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            Console.WriteLine($"[AuthService] POST {apiUrl}");
            Console.WriteLine($"[AuthService] Request: {jsonContent}");

            var response = await client.PostAsync(apiUrl, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[AuthService] Response: {responseContent}");

            // WebSube-API wrapper response parsing
            var (success, data, error) = ParseApiResponse(responseContent);

            if (!success || data == null)
            {
                return new TcknGsmResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Message = error ?? "API yanıtı işlenemedi"
                };
            }

            // Parse inner data
            var dataElement = data.Value;
            
            var result = new TcknGsmResponse
            {
                Success = dataElement.TryGetProperty("success", out var s) && s.GetBoolean(),
                StatusCode = dataElement.TryGetProperty("statusCode", out var sc) ? sc.GetInt32() : 200,
                Message = dataElement.TryGetProperty("message", out var m) ? m.GetString() ?? "" : ""
            };

            if (dataElement.TryGetProperty("value", out var valueProp) && valueProp.ValueKind == JsonValueKind.Object)
            {
                result.Value = new TcknGsmValue
                {
                    CustomerId = valueProp.TryGetProperty("CustomerId", out var cid) ? cid.GetInt32() : 0,
                    TCKN = valueProp.TryGetProperty("TCKN", out var t) ? t.GetString() ?? "" : "",
                    GSM = valueProp.TryGetProperty("GSM", out var g) ? g.GetString() ?? "" : ""
                };
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] ValidateTcknGsm Error: {ex.Message}");
            return new TcknGsmResponse
            {
                Success = false,
                StatusCode = 500,
                Message = $"Bağlantı hatası: {ex.Message}"
            };
        }
    }

    #endregion

    #region OTP Operations

    public async Task<SendOtpResponse> GenerateOtp(string tckn, string gsm)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            
            // Use WebSube-API proxy (Next.js)
            var apiUrl = $"{_apiBaseUrl}/generate-otp";
            
            // GSM formatını düzelt (05 ile başlamalı)
            if (!gsm.StartsWith("0"))
            {
                gsm = "0" + gsm;
            }

            var requestData = new { tckn, gsm, utmId = "5" };
            var jsonContent = JsonSerializer.Serialize(requestData);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            Console.WriteLine($"[AuthService] POST {apiUrl}");
            Console.WriteLine($"[AuthService] Request: {jsonContent}");

            var response = await client.PostAsync(apiUrl, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[AuthService] GenerateOtp Response: {responseContent}");

            // WebSube-API wrapper response parsing
            var (success, data, error) = ParseApiResponse(responseContent);

            if (!success || data == null)
            {
                return new SendOtpResponse
                {
                    Success = false,
                    Message = error ?? "OTP oluşturulamadı"
                };
            }

            // Parse inner data
            var dataElement = data.Value;
            
            var isSuccess = dataElement.TryGetProperty("success", out var s) && s.GetBoolean();
            var message = dataElement.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "";

            if (!isSuccess)
            {
                return new SendOtpResponse
                {
                    Success = false,
                    Message = message
                };
            }

            // Get value
            if (dataElement.TryGetProperty("value", out var valueProp) && valueProp.ValueKind == JsonValueKind.Object)
            {
                var otpCode = valueProp.TryGetProperty("OTPCode", out var otp) ? otp.GetInt32() : 0;
                var otpId = valueProp.TryGetProperty("OtpId", out var oid) ? oid.GetInt32() : 0;
                var smsVerificationId = valueProp.TryGetProperty("SmsVerificationId", out var svid) ? svid.GetInt32() : 0;

                return new SendOtpResponse
                {
                    Success = true,
                    Message = message,
                    OtpCode = otpCode.ToString(),
                    OtpId = otpId,
                    SmsVerificationId = smsVerificationId
                };
            }

            return new SendOtpResponse
            {
                Success = false,
                Message = "OTP değerleri alınamadı"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] GenerateOtp Error: {ex.Message}");
            return new SendOtpResponse
            {
                Success = false,
                Message = $"Bağlantı hatası: {ex.Message}"
            };
        }
    }

    public async Task<bool> SendOtpSms(string gsm, string otpCode)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            
            // Use WebSube-API proxy (Next.js)
            var apiUrl = $"{_apiBaseUrl}/send-otp-sms";
            
            // GSM formatını düzelt (0 olmadan gönderilmeli: 5064795407)
            if (gsm.StartsWith("0"))
            {
                gsm = gsm.Substring(1);
            }

            var requestData = new { gsm, otpCode };
            var jsonContent = JsonSerializer.Serialize(requestData);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            Console.WriteLine($"[AuthService] POST {apiUrl}");
            Console.WriteLine($"[AuthService] Request: {jsonContent}");

            var response = await client.PostAsync(apiUrl, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[AuthService] SendOtpSms Response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[AuthService] SMS başarıyla gönderildi: {gsm}");
                return true;
            }
            
            Console.WriteLine($"[AuthService] SMS gönderilemedi: Status {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] SendOtpSms Error: {ex.Message}");
            return false;
        }
    }

    public async Task<VerifyOtpResponse> VerifyOtp(string otpCode)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            
            // Use WebSube-API proxy (Next.js)
            var apiUrl = $"{_apiBaseUrl}/verify-otp";
            
            var requestData = new { otpCode };
            var jsonContent = JsonSerializer.Serialize(requestData);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            Console.WriteLine($"[AuthService] POST {apiUrl}");
            Console.WriteLine($"[AuthService] Request: {jsonContent}");

            var response = await client.PostAsync(apiUrl, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[AuthService] VerifyOtp Response: {responseContent}");

            // WebSube-API wrapper response parsing
            var (success, data, error) = ParseApiResponse(responseContent);

            if (!success || data == null)
            {
                return new VerifyOtpResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Message = error ?? "API yanıtı işlenemedi"
                };
            }

            // Parse inner data
            var dataElement = data.Value;
            
            return new VerifyOtpResponse
            {
                Success = dataElement.TryGetProperty("success", out var s) && s.GetBoolean(),
                StatusCode = dataElement.TryGetProperty("statusCode", out var sc) ? sc.GetInt32() : 200,
                Message = dataElement.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "",
                Value = dataElement.TryGetProperty("value", out var v) ? v.Clone() : null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] VerifyOtp Error: {ex.Message}");
            return new VerifyOtpResponse
            {
                Success = false,
                StatusCode = 500,
                Message = $"Bağlantı hatası: {ex.Message}"
            };
        }
    }

    public async Task<SendOtpResponse> SendOtp(string tckn, string gsm)
    {
        // Step 1: Generate OTP
        var generateResult = await GenerateOtp(tckn, gsm);
        
        if (!generateResult.Success)
        {
            return generateResult;
        }

        // Step 2: Send SMS - SMS gönderilmezse hata döndür
        var smsResult = await SendOtpSms(gsm, generateResult.OtpCode!);
        
        if (!smsResult)
        {
            Console.WriteLine("[AuthService] SMS gönderilemedi!");
            return new SendOtpResponse
            {
                Success = false,
                Message = "SMS gönderilemedi. Lütfen GSM numaranızı kontrol edin ve tekrar deneyin."
            };
        }

        Console.WriteLine($"[AuthService] SMS başarıyla gönderildi: {gsm}");
        return generateResult;
    }

    public IssueTokenResponse IssueToken(string identifier)
    {
        var token = Guid.NewGuid().ToString();
        var expiry = DateTime.Now.AddHours(24);

        return new IssueTokenResponse
        {
            Success = true,
            Token = token,
            Expiry = expiry
        };
    }

    #endregion

    #region KVKK Operations

    public async Task<KvkkTextResponse?> GetKvkkText(int kvkkId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            
            // Use WebSube-API proxy (Next.js)
            var apiUrl = $"{_apiBaseUrl}/kvkk-text/{kvkkId}";
            
            Console.WriteLine($"[AuthService] GET {apiUrl}");

            var response = await client.GetAsync(apiUrl);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[AuthService] GetKvkkText Response: {responseContent}");

            // WebSube-API wrapper response parsing
            var (success, data, error) = ParseApiResponse(responseContent);

            if (!success || data == null)
            {
                return null;
            }

            var dataElement = data.Value;
            
            return new KvkkTextResponse
            {
                Id = dataElement.TryGetProperty("Id", out var id) ? id.GetInt32() : 
                     (dataElement.TryGetProperty("id", out var id2) ? id2.GetInt32() : 0),
                Name = dataElement.TryGetProperty("Name", out var n) ? n.GetString() ?? "" : 
                       (dataElement.TryGetProperty("name", out var n2) ? n2.GetString() ?? "" : ""),
                Text = dataElement.TryGetProperty("Text", out var t) ? t.GetString() ?? "" : 
                       (dataElement.TryGetProperty("text", out var t2) ? t2.GetString() ?? "" : "")
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] GetKvkkText Error: {ex.Message}");
            return null;
        }
    }

    public async Task<KvkkOnayResponse?> SaveKvkkOnay(int kvkkId, int customerId, bool isOk)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            
            // Use WebSube-API proxy (Next.js)
            var apiUrl = $"{_apiBaseUrl}/kvkk-onay";
            
            var requestData = new { kvkkId, customerId, isOk };
            var jsonContent = JsonSerializer.Serialize(requestData);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            Console.WriteLine($"[AuthService] POST {apiUrl}");
            Console.WriteLine($"[AuthService] Request: {jsonContent}");

            var response = await client.PostAsync(apiUrl, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[AuthService] SaveKvkkOnay Response: {responseContent}");

            // WebSube-API wrapper response parsing
            var (success, data, error) = ParseApiResponse(responseContent);

            if (!success || data == null)
            {
                return null;
            }

            var dataElement = data.Value;
            
            return new KvkkOnayResponse
            {
                Id = dataElement.TryGetProperty("Id", out var id) ? id.GetInt32() : 
                     (dataElement.TryGetProperty("id", out var id2) ? id2.GetInt32() : 0),
                Message = dataElement.TryGetProperty("Message", out var m) ? m.GetString() ?? "" : 
                          (dataElement.TryGetProperty("message", out var m2) ? m2.GetString() ?? "" : "")
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] SaveKvkkOnay Error: {ex.Message}");
            return null;
        }
    }

    #endregion
}
