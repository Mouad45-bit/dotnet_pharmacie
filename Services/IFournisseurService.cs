namespace project_pharmacie.Services;

public interface IFournisseurService
{
    Task<SupplierListVm> GetDashboardAsync(string? sort);

    Task<SupplierRateVm?> GetRatePageAsync(string supplierId);
    Task<ServiceResult> RateAsync(string supplierId, int rating, string? comment);
}