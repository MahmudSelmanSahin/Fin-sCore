using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Fin_sCore.Services;

namespace Fin_sCore.Pages;

public class ProfileModel : PageModel
{
    private readonly UserService _userService;

    public ProfileModel(UserService userService)
    {
        _userService = userService;
    }

    // Demo CustomerId - gerçek uygulamada session'dan alınır
    public int CustomerId { get; set; } = 1000849;
    public string UserName { get; set; } = "Kullanıcı";

    // Address Info
    public AddressInfo? AddressInfo { get; set; }
    public bool AddressLoaded { get; set; }
    public string? AddressError { get; set; }

    // Job Info
    public JobInfo? JobInfo { get; set; }
    public bool JobLoaded { get; set; }
    public string? JobError { get; set; }

    // Wife/Spouse Info
    public WifeInfo? WifeInfo { get; set; }
    public bool WifeLoaded { get; set; }
    public string? WifeError { get; set; }

    // Finance/Assets Info
    public FinanceInfo? FinanceInfo { get; set; }
    public bool FinanceLoaded { get; set; }
    public string? FinanceError { get; set; }

    // Form Bindings
    [BindProperty]
    public AddressUpdateRequest AddressForm { get; set; } = new();

    [BindProperty]
    public JobUpdateRequest JobForm { get; set; } = new();

    [BindProperty]
    public WifeUpdateRequest WifeForm { get; set; } = new();

    [BindProperty]
    public FinanceUpdateRequest FinanceForm { get; set; } = new();

    // Cities and Towns (demo data - gerçek uygulamada API'den gelir)
    public List<SelectItem> Cities { get; set; } = new()
    {
        new SelectItem { Id = 34, Name = "İstanbul" },
        new SelectItem { Id = 6, Name = "Ankara" },
        new SelectItem { Id = 35, Name = "İzmir" },
        new SelectItem { Id = 16, Name = "Bursa" },
        new SelectItem { Id = 7, Name = "Antalya" }
    };

    public List<SelectItem> Towns { get; set; } = new()
    {
        new SelectItem { Id = 1, Name = "Kadıköy" },
        new SelectItem { Id = 2, Name = "Beşiktaş" },
        new SelectItem { Id = 3, Name = "Şişli" },
        new SelectItem { Id = 4, Name = "Üsküdar" },
        new SelectItem { Id = 5, Name = "Bakırköy" },
        new SelectItem { Id = 12, Name = "Ataşehir" }
    };

    // Job Categories
    public List<SelectItem> WorkTypes { get; set; } = new()
    {
        new SelectItem { Id = 1, Name = "Çalışan" },
        new SelectItem { Id = 2, Name = "Serbest Meslek" },
        new SelectItem { Id = 3, Name = "Emekli" },
        new SelectItem { Id = 4, Name = "Öğrenci" },
        new SelectItem { Id = 5, Name = "İşsiz" }
    };

    public List<SelectItem> JobGroups { get; set; } = new()
    {
        new SelectItem { Id = 1, Name = "Bilişim / Teknoloji" },
        new SelectItem { Id = 2, Name = "Finans / Bankacılık" },
        new SelectItem { Id = 3, Name = "Sağlık" },
        new SelectItem { Id = 4, Name = "Eğitim" },
        new SelectItem { Id = 5, Name = "Üretim / Sanayi" },
        new SelectItem { Id = 6, Name = "Hizmet Sektörü" },
        new SelectItem { Id = 7, Name = "Kamu" }
    };

    // Work Sectors (for Finance)
    public List<SelectItem> WorkSectors { get; set; } = new()
    {
        new SelectItem { Id = 1, Name = "Özel Sektör" },
        new SelectItem { Id = 2, Name = "Kamu" },
        new SelectItem { Id = 3, Name = "Serbest Meslek" },
        new SelectItem { Id = 4, Name = "Emekli" }
    };

    // Banks
    public List<string> Banks { get; set; } = new()
    {
        "Ziraat Bankası",
        "Vakıfbank",
        "Halkbank",
        "İş Bankası",
        "Garanti BBVA",
        "Yapı Kredi",
        "Akbank",
        "QNB Finansbank",
        "Denizbank",
        "TEB",
        "ING",
        "HSBC",
        "Diğer"
    };

    public async Task<IActionResult> OnGetAsync()
    {
        // Authentication kontrolü
        var authToken = HttpContext.Session.GetString("AuthToken");
        if (string.IsNullOrEmpty(authToken))
        {
            return RedirectToPage("/Index");
        }

        // Session'dan CustomerId al
        var sessionCustomerId = HttpContext.Session.GetInt32("CustomerId");
        if (sessionCustomerId.HasValue && sessionCustomerId.Value > 0)
        {
            CustomerId = sessionCustomerId.Value;
        }

        await LoadAddressInfoAsync();
        await LoadJobInfoAsync();
        await LoadWifeInfoAsync();
        await LoadFinanceInfoAsync();
        
        return Page();
    }

    private async Task LoadAddressInfoAsync()
    {
        var result = await _userService.GetAddressInfoAsync(CustomerId);
        if (result.Success && result.Value != null)
        {
            AddressInfo = result.Value;
            AddressLoaded = true;

            // Form'u doldur
            AddressForm = new AddressUpdateRequest
            {
                CustomerId = AddressInfo.CustomerId,
                Adress = AddressInfo.Adress,
                CityId = AddressInfo.CityId,
                TownId = AddressInfo.TownId,
                Source = 2
            };
        }
        else
        {
            AddressError = result.Message;
            AddressLoaded = false;
        }
    }

    private async Task LoadJobInfoAsync()
    {
        var result = await _userService.GetJobInfoAsync(CustomerId);
        if (result.Success && result.Value != null)
        {
            JobInfo = result.Value;
            JobLoaded = true;

            // Form'u doldur
            JobForm = new JobUpdateRequest
            {
                CustomerId = JobInfo.CustomerId,
                CustomerWork = JobInfo.CustomerWork,
                JobGroupId = JobInfo.JobGroupId,
                WorkingYears = JobInfo.WorkingYears,
                WorkingMonth = JobInfo.WorkingMonth,
                TitleCompany = JobInfo.TitleCompany,
                CompanyPosition = JobInfo.CompanyPosition
            };
        }
        else
        {
            JobError = result.Message;
            JobLoaded = false;
        }
    }

    private async Task LoadWifeInfoAsync()
    {
        var result = await _userService.GetWifeInfoAsync(CustomerId);
        if (result.Success && result.Value != null)
        {
            WifeInfo = result.Value;
            WifeLoaded = true;

            // Form'u doldur
            WifeForm = new WifeUpdateRequest
            {
                CustomerId = WifeInfo.CustomerId,
                MaritalStatus = WifeInfo.MaritalStatus,
                WorkWife = WifeInfo.WorkWife,
                WifeSalaryAmount = WifeInfo.WifeSalaryAmount
            };
        }
        else
        {
            WifeError = result.Message;
            WifeLoaded = false;
        }
    }

    private async Task LoadFinanceInfoAsync()
    {
        var result = await _userService.GetFinanceInfoAsync(CustomerId);
        if (result.Success && result.Value != null)
        {
            FinanceInfo = result.Value;
            FinanceLoaded = true;

            // Form'u doldur
            FinanceForm = new FinanceUpdateRequest
            {
                CustomerId = FinanceInfo.CustomerId,
                WorkSector = FinanceInfo.WorkSector,
                SalaryBank = FinanceInfo.SalaryBank,
                SalaryAmount = FinanceInfo.SalaryAmount,
                CarStatus = FinanceInfo.CarStatus,
                HouseStatus = FinanceInfo.HouseStatus
            };
        }
        else
        {
            FinanceError = result.Message;
            FinanceLoaded = false;
        }
    }

    public async Task<IActionResult> OnPostUpdateAddressAsync()
    {
        AddressForm.CustomerId = CustomerId;
        AddressForm.Source = 2;

        var result = await _userService.UpdateAddressInfoAsync(AddressForm);

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Adres bilgileri başarıyla güncellendi.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? "Güncelleme sırasında hata oluştu.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateJobAsync()
    {
        JobForm.CustomerId = CustomerId;

        var result = await _userService.UpdateJobInfoAsync(JobForm);

        if (result.Success)
        {
            TempData["SuccessMessage"] = "İş bilgileri başarıyla güncellendi.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? "Güncelleme sırasında hata oluştu.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateWifeAsync()
    {
        WifeForm.CustomerId = CustomerId;

        var result = await _userService.UpdateWifeInfoAsync(WifeForm);

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Eş bilgileri başarıyla güncellendi.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? "Güncelleme sırasında hata oluştu.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateFinanceAsync()
    {
        FinanceForm.CustomerId = CustomerId;

        var result = await _userService.UpdateFinanceInfoAsync(FinanceForm);

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Finansal bilgiler başarıyla güncellendi.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? "Güncelleme sırasında hata oluştu.";
        }

        return RedirectToPage();
    }
}

public class SelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

