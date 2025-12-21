using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace Fin_sCore.Services;

public class UserService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    
    // API Configuration - loaded from appsettings
    private string BaseUrl => _configuration["CustomerApi:BaseUrl"] ?? "https://customers-api.azurewebsites.net/api/customer";
    private string AddressGetCode => _configuration["CustomerApi:Codes:AddressGet"] ?? "";
    private string AddressUpdateCode => _configuration["CustomerApi:Codes:AddressUpdate"] ?? "";
    private string JobInfoGetCode => _configuration["CustomerApi:Codes:JobInfoGet"] ?? "";
    private string JobInfoUpdateCode => _configuration["CustomerApi:Codes:JobInfoUpdate"] ?? "";
    private string WifeInfoGetCode => _configuration["CustomerApi:Codes:WifeInfoGet"] ?? "";
    private string WifeInfoUpdateCode => _configuration["CustomerApi:Codes:WifeInfoUpdate"] ?? "";
    private string FinanceGetCode => _configuration["CustomerApi:Codes:FinanceGet"] ?? "";
    private string FinanceUpdateCode => _configuration["CustomerApi:Codes:FinanceUpdate"] ?? "";

    public UserService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    #region Address/Contact Info

    /// <summary>
    /// Müşteri adres bilgilerini getirir
    /// </summary>
    public async Task<ApiResponse<AddressInfo>> GetAddressInfoAsync(int customerId)
    {
        try
        {
            var url = $"{BaseUrl}/addressfull/{customerId}?code={AddressGetCode}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AddressInfo>();
                return new ApiResponse<AddressInfo>
                {
                    Success = true,
                    Message = "Adres bilgileri başarıyla getirildi",
                    Value = result
                };
            }

            return new ApiResponse<AddressInfo>
            {
                Success = false,
                Message = $"API Hatası: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<AddressInfo>
            {
                Success = false,
                Message = $"Bağlantı hatası: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Müşteri adres bilgilerini günceller
    /// </summary>
    public async Task<ApiResponse<AddressInfo>> UpdateAddressInfoAsync(AddressUpdateRequest request)
    {
        try
        {
            var url = $"{BaseUrl}/address?code={AddressUpdateCode}";
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<AddressInfo>>();
                return result ?? new ApiResponse<AddressInfo> { Success = true, Message = "Adres bilgileri güncellendi" };
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ApiResponse<AddressInfo>
            {
                Success = false,
                Message = $"Güncelleme başarısız: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<AddressInfo>
            {
                Success = false,
                Message = $"Bağlantı hatası: {ex.Message}"
            };
        }
    }

    #endregion

    #region Job/Income Info

    /// <summary>
    /// Müşteri iş bilgilerini getirir
    /// </summary>
    public async Task<ApiResponse<JobInfo>> GetJobInfoAsync(int customerId)
    {
        try
        {
            var url = $"{BaseUrl}/job-infonew/{customerId}?code={JobInfoGetCode}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JobInfo>();
                return new ApiResponse<JobInfo>
                {
                    Success = true,
                    Message = "İş bilgileri başarıyla getirildi",
                    Value = result
                };
            }

            return new ApiResponse<JobInfo>
            {
                Success = false,
                Message = $"API Hatası: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<JobInfo>
            {
                Success = false,
                Message = $"Bağlantı hatası: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Müşteri iş bilgilerini günceller
    /// </summary>
    public async Task<ApiResponse<JobInfo>> UpdateJobInfoAsync(JobUpdateRequest request)
    {
        try
        {
            var url = $"{BaseUrl}/job-profile?code={JobInfoUpdateCode}";
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<JobInfo>>();
                return result ?? new ApiResponse<JobInfo> { Success = true, Message = "İş bilgileri güncellendi" };
            }

            return new ApiResponse<JobInfo>
            {
                Success = false,
                Message = $"Güncelleme başarısız: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<JobInfo>
            {
                Success = false,
                Message = $"Bağlantı hatası: {ex.Message}"
            };
        }
    }

    #endregion

    #region Wife/Spouse Info

    /// <summary>
    /// Eş gelir/çalışma bilgilerini getirir
    /// </summary>
    public async Task<ApiResponse<WifeInfo>> GetWifeInfoAsync(int customerId)
    {
        try
        {
            var url = $"{BaseUrl}/wife-info/{customerId}?code={WifeInfoGetCode}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<WifeInfo>();
                return new ApiResponse<WifeInfo>
                {
                    Success = true,
                    Message = "Eş bilgileri başarıyla getirildi",
                    Value = result
                };
            }

            return new ApiResponse<WifeInfo>
            {
                Success = false,
                Message = $"API Hatası: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<WifeInfo>
            {
                Success = false,
                Message = $"Bağlantı hatası: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Eş gelir/çalışma bilgilerini günceller
    /// </summary>
    public async Task<ApiResponse<WifeInfo>> UpdateWifeInfoAsync(WifeUpdateRequest request)
    {
        try
        {
            var url = $"{BaseUrl}/wife-info?code={WifeInfoUpdateCode}";
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<WifeInfo>>();
                return result ?? new ApiResponse<WifeInfo> { Success = true, Message = "Eş bilgileri güncellendi" };
            }

            return new ApiResponse<WifeInfo>
            {
                Success = false,
                Message = $"Güncelleme başarısız: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<WifeInfo>
            {
                Success = false,
                Message = $"Bağlantı hatası: {ex.Message}"
            };
        }
    }

    #endregion

    #region Finance/Assets Info

    /// <summary>
    /// Finansal bilgileri getirir
    /// </summary>
    public async Task<ApiResponse<FinanceInfo>> GetFinanceInfoAsync(int customerId)
    {
        try
        {
            var url = $"{BaseUrl}/finance-assets/{customerId}?code={FinanceGetCode}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<FinanceInfo>();
                return new ApiResponse<FinanceInfo>
                {
                    Success = true,
                    Message = "Finansal bilgiler başarıyla getirildi",
                    Value = result
                };
            }

            return new ApiResponse<FinanceInfo>
            {
                Success = false,
                Message = $"API Hatası: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<FinanceInfo>
            {
                Success = false,
                Message = $"Bağlantı hatası: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Finansal bilgileri günceller
    /// </summary>
    public async Task<ApiResponse<FinanceInfo>> UpdateFinanceInfoAsync(FinanceUpdateRequest request)
    {
        try
        {
            var url = $"{BaseUrl}/finance-assets?code={FinanceUpdateCode}";
            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<FinanceInfo>>();
                return result ?? new ApiResponse<FinanceInfo> { Success = true, Message = "Finansal bilgiler güncellendi" };
            }

            return new ApiResponse<FinanceInfo>
            {
                Success = false,
                Message = $"Güncelleme başarısız: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<FinanceInfo>
            {
                Success = false,
                Message = $"Bağlantı hatası: {ex.Message}"
            };
        }
    }

    #endregion
}

#region DTOs

public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("value")]
    public T? Value { get; set; }
}

// Address Models
public class AddressInfo
{
    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("adress")]
    public string? Adress { get; set; }

    [JsonPropertyName("cityId")]
    public int CityId { get; set; }

    [JsonPropertyName("townId")]
    public int TownId { get; set; }

    [JsonPropertyName("employeeId")]
    public int? EmployeeId { get; set; }

    [JsonPropertyName("source")]
    public int Source { get; set; }
}

public class AddressUpdateRequest
{
    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("adress")]
    public string? Adress { get; set; }

    [JsonPropertyName("cityId")]
    public int CityId { get; set; }

    [JsonPropertyName("townId")]
    public int TownId { get; set; }

    [JsonPropertyName("source")]
    public int Source { get; set; } = 2; // 2 = kişi ekler
}

// Job Models
public class JobInfo
{
    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("customerWork")]
    public int CustomerWork { get; set; }

    [JsonPropertyName("jobGroupId")]
    public int JobGroupId { get; set; }

    [JsonPropertyName("workingYears")]
    public int WorkingYears { get; set; }

    [JsonPropertyName("workingMonth")]
    public int WorkingMonth { get; set; }

    [JsonPropertyName("titleCompany")]
    public string? TitleCompany { get; set; }

    [JsonPropertyName("companyPosition")]
    public string? CompanyPosition { get; set; }
}

public class JobUpdateRequest
{
    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("customerWork")]
    public int CustomerWork { get; set; }

    [JsonPropertyName("jobGroupId")]
    public int JobGroupId { get; set; }

    [JsonPropertyName("workingYears")]
    public int WorkingYears { get; set; }

    [JsonPropertyName("workingMonth")]
    public int WorkingMonth { get; set; }

    [JsonPropertyName("titleCompany")]
    public string? TitleCompany { get; set; }

    [JsonPropertyName("companyPosition")]
    public string? CompanyPosition { get; set; }
}

// Wife/Spouse Models
public class WifeInfo
{
    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("maritalStatus")]
    public bool MaritalStatus { get; set; } // true = Evli, false = Bekar

    [JsonPropertyName("workWife")]
    public bool WorkWife { get; set; } // true = Çalışıyor, false = Çalışmıyor

    [JsonPropertyName("wifeSalaryAmount")]
    public decimal WifeSalaryAmount { get; set; }
}

public class WifeUpdateRequest
{
    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("maritalStatus")]
    public bool MaritalStatus { get; set; }

    [JsonPropertyName("workWife")]
    public bool WorkWife { get; set; }

    [JsonPropertyName("wifeSalaryAmount")]
    public decimal WifeSalaryAmount { get; set; }
}

// Finance/Assets Models
public class FinanceInfo
{
    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("workSector")]
    public int WorkSector { get; set; }

    [JsonPropertyName("salaryBank")]
    public string? SalaryBank { get; set; }

    [JsonPropertyName("salaryAmount")]
    public decimal SalaryAmount { get; set; }

    [JsonPropertyName("carStatus")]
    public bool CarStatus { get; set; }

    [JsonPropertyName("houseStatus")]
    public bool HouseStatus { get; set; }
}

public class FinanceUpdateRequest
{
    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("workSector")]
    public int WorkSector { get; set; }

    [JsonPropertyName("salaryBank")]
    public string? SalaryBank { get; set; }

    [JsonPropertyName("salaryAmount")]
    public decimal SalaryAmount { get; set; }

    [JsonPropertyName("carStatus")]
    public bool CarStatus { get; set; }

    [JsonPropertyName("houseStatus")]
    public bool HouseStatus { get; set; }
}

#endregion

