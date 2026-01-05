using project_pharmacie.Models;

namespace project_pharmacie.Services;

public interface IProduitService
{
    // Liste (option recherche)
    Task<List<Produit>> ListAsync(string? query = null);

    // Détail
    Task<Produit?> GetByReferenceAsync(string reference);

    // CRUD (utile pour refactor Pages Products)
    Task<ServiceResult> CreateAsync(Produit produit);
    Task<ServiceResult> UpdateAsync(Produit produit);
    Task<ServiceResult> DeleteAsync(string reference);

    // Stock
    Task<ServiceResult> DecreaseStockAsync(string reference, int quantity);
}