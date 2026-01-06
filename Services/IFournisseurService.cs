namespace project_pharmacie.Services;

public interface IFournisseurService
{
    Task<SupplierListVm> GetDashboardAsync(string? sort);
}