using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Staff.Pages.Suppliers;

public class IndexModel : PageModel
{
    private readonly IFournisseurService _suppliers;

    public IndexModel(IFournisseurService suppliers) => _suppliers = suppliers;

    [BindProperty(SupportsGet = true)]
    public string? Sort { get; set; } = "rating_desc";

    public List<SupplierRowVm> Rows { get; private set; } = new();

    public int TotalSuppliers { get; private set; }
    public double AverageRating { get; private set; }
    public int TotalRatingsCount { get; private set; }
    public string BestSupplierName { get; private set; } = "-";
    public double BestSupplierRating { get; private set; }

    public async Task OnGetAsync()
    {
        var vm = await _suppliers.GetDashboardAsync(Sort);

        Rows = vm.Rows;
        TotalSuppliers = vm.TotalSuppliers;
        AverageRating = vm.AverageRating;
        TotalRatingsCount = vm.TotalRatingsCount;
        BestSupplierName = vm.BestSupplierName;
        BestSupplierRating = vm.BestSupplierRating;
    }

    public (string badgeCls, string text) GetRatingBadge(double rating)
    {
        if (rating >= 4.5)
            return ("bg-emerald-50 text-emerald-700 ring-emerald-200", "Excellent");
        if (rating >= 3.5)
            return ("bg-amber-50 text-amber-700 ring-amber-200", "Bon");
        return ("bg-red-50 text-red-700 ring-red-200", "À surveiller");
    }
}