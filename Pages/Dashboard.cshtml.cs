using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Fin_sCore.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;
        private readonly IWebHostEnvironment _environment;

        public DashboardModel(ILogger<DashboardModel> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        // Kullanıcı Bilgileri (API'den gelecek - simüle edildi)
        public string UserName { get; set; } = "Ahmet Yılmaz";
        public string UserPhone { get; set; } = "0532 XXX XX XX";
        public decimal TotalCreditLimit { get; set; } = 150000.00m;
        public decimal UsedCreditLimit { get; set; } = 45000.00m;
        public decimal AvailableCreditLimit => TotalCreditLimit - UsedCreditLimit;
        
        // Bu ayki ödenmesi gereken toplam miktar
        public decimal MonthlyPaymentDue => ActiveLoans.Sum(l => l.MonthlyPayment);

        // Aktif Krediler (API'den gelecek)
        public List<ActiveLoanModel> ActiveLoans { get; set; } = new();

        // Yardım & Destek Merkezi (CMS JSON'dan gelecek)
        public List<HelpCenterItem> HelpCenterItems { get; set; } = new();

        // SSS (Sıkça Sorulan Sorular)
        public List<FaqItem> FaqItems { get; set; } = new();

        public IActionResult OnGet()
        {
            // Authentication kontrolü
            var authToken = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToPage("/Index");
            }

            LoadUserData();
            LoadHelpCenterData();
            LoadFaqData();
            return Page();
        }

        private void LoadUserData()
        {
            // TODO: API'den gerçek veri çekilecek
            ActiveLoans = new List<ActiveLoanModel>
            {
                new ActiveLoanModel
                {
                    LoanType = "İhtiyaç Kredisi",
                    BankName = "Garanti BBVA",
                    BankPaymentUrl = "https://www.garantibbva.com.tr/bireysel/kredi-karti/kredi-karti-odeme",
                    LoanAmount = 25000.00m,
                    RemainingAmount = 18500.00m,
                    MonthlyPayment = 1250.00m,
                    NextPaymentDate = new DateTime(2025, 1, 15),
                    InstallmentNumber = 12,
                    RemainingInstallments = 8
                },
                new ActiveLoanModel
                {
                    LoanType = "Konut Kredisi",
                    BankName = "Yapı Kredi",
                    BankPaymentUrl = "https://www.yapikredi.com.tr/bireysel-bankacilik/krediler",
                    LoanAmount = 20000.00m,
                    RemainingAmount = 15000.00m,
                    MonthlyPayment = 800.00m,
                    NextPaymentDate = new DateTime(2025, 1, 20),
                    InstallmentNumber = 24,
                    RemainingInstallments = 18
                }
            };
        }

        private void LoadHelpCenterData()
        {
            try
            {
                var jsonPath = Path.Combine(_environment.WebRootPath, "data", "help_center.json");
                
                if (System.IO.File.Exists(jsonPath))
                {
                    var jsonString = System.IO.File.ReadAllText(jsonPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    HelpCenterItems = JsonSerializer.Deserialize<List<HelpCenterItem>>(jsonString, options) ?? new List<HelpCenterItem>();
                }
                else
                {
                    // Fallback data
                    HelpCenterItems = GetDefaultHelpCenterItems();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yardım merkezi verileri yüklenirken hata oluştu");
                HelpCenterItems = GetDefaultHelpCenterItems();
            }
        }

        private List<HelpCenterItem> GetDefaultHelpCenterItems()
        {
            return new List<HelpCenterItem>
            {
                new HelpCenterItem
                {
                    Title = "Kredi Başvurusu Nasıl Yapılır?",
                    Description = "Kredi başvuru sürecini adım adım öğrenin",
                    Icon = "document",
                    Category = "Kredi İşlemleri",
                    Link = "/help/credit-application"
                },
                new HelpCenterItem
                {
                    Title = "Kredi Hesaplama",
                    Description = "Aylık taksit tutarınızı hesaplayın",
                    Icon = "calculator",
                    Category = "Hesaplama Araçları",
                    Link = "/calculator"
                },
                new HelpCenterItem
                {
                    Title = "Sıkça Sorulan Sorular",
                    Description = "En çok merak edilen soruların cevapları",
                    Icon = "help",
                    Category = "Destek",
                    Link = "/faq"
                },
                new HelpCenterItem
                {
                    Title = "İletişim",
                    Description = "Müşteri hizmetlerimize ulaşın",
                    Icon = "phone",
                    Category = "Destek",
                    Link = "/contact"
                }
            };
        }

        private void LoadFaqData()
        {
            try
            {
                var jsonPath = Path.Combine(_environment.WebRootPath, "data", "faq.json");
                
                if (System.IO.File.Exists(jsonPath))
                {
                    var jsonString = System.IO.File.ReadAllText(jsonPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    FaqItems = JsonSerializer.Deserialize<List<FaqItem>>(jsonString, options) ?? new List<FaqItem>();
                    FaqItems = FaqItems.Where(f => f.IsActive).OrderBy(f => f.Order).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSS verileri yüklenirken hata oluştu");
                FaqItems = new List<FaqItem>();
            }
        }
    }

    public class ActiveLoanModel
    {
        public string LoanType { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankPaymentUrl { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal MonthlyPayment { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public int InstallmentNumber { get; set; }
        public int RemainingInstallments { get; set; }
        public decimal CompletionPercentage => ((decimal)(InstallmentNumber - RemainingInstallments) / InstallmentNumber) * 100;
    }

    public class HelpCenterItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public bool IsClickable { get; set; } = true;
    }

    public class FaqItem
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
