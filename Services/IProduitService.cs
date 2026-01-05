using project_pharmacie.Models;

namespace project_pharmacie.Services;

public interface IProduitService
{
    Task<List<Produit>> ListAsync(string? query = null);
    Task<Produit?> GetByReferenceAsync(string reference);

    // Stock
    Task<ServiceResult> DecreaseStockAsync(string reference, int quantity);
}