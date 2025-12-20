namespace Fin_sCore.Services;

public class AuthService
{
    // Simulated OTP storage (in real app, use database/cache)
    private static Dictionary<string, string> _otpStorage = new();
    private static Dictionary<string, DateTime> _otpExpiry = new();

    public class SendOtpResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? OtpCode { get; set; } // For demo purposes only
    }

    public class VerifyOtpResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
    }

    public class IssueTokenResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public DateTime Expiry { get; set; }
    }

    public SendOtpResponse SendOtp(string identifier)
    {
        // Generate 6-digit OTP
        var random = new Random();
        var otp = random.Next(100000, 999999).ToString();

        // Store OTP (expires in 5 minutes)
        _otpStorage[identifier] = otp;
        _otpExpiry[identifier] = DateTime.Now.AddMinutes(5);

        // In real app, send SMS here
        // For demo, we'll return the OTP code
        return new SendOtpResponse
        {
            Success = true,
            Message = "OTP başarıyla gönderildi",
            OtpCode = otp // Only for demo - remove in production
        };
    }

    public VerifyOtpResponse VerifyOtp(string identifier, string otpCode)
    {
        if (!_otpStorage.ContainsKey(identifier))
        {
            return new VerifyOtpResponse
            {
                Success = false,
                Message = "OTP bulunamadı. Lütfen yeni kod talep edin."
            };
        }

        if (_otpExpiry[identifier] < DateTime.Now)
        {
            _otpStorage.Remove(identifier);
            _otpExpiry.Remove(identifier);
            return new VerifyOtpResponse
            {
                Success = false,
                Message = "OTP süresi dolmuş. Lütfen yeni kod talep edin."
            };
        }

        if (_otpStorage[identifier] != otpCode)
        {
            return new VerifyOtpResponse
            {
                Success = false,
                Message = "Hatalı OTP kodu"
            };
        }

        // OTP verified successfully
        _otpStorage.Remove(identifier);
        _otpExpiry.Remove(identifier);

        return new VerifyOtpResponse
        {
            Success = true,
            Message = "OTP doğrulandı",
            Token = Guid.NewGuid().ToString()
        };
    }

    public IssueTokenResponse IssueToken(string identifier)
    {
        // Generate session token
        var token = Guid.NewGuid().ToString();
        var expiry = DateTime.Now.AddHours(24);

        return new IssueTokenResponse
        {
            Success = true,
            Token = token,
            Expiry = expiry
        };
    }
}

