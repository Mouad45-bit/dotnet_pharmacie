using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace project_pharmacie.Services;

public interface ICommandeService
{
    // Création
    Task<ServiceResult<OrderCreatedInfo>> CreateAsync(string supplierId, string productRef, int quantity);

    // Historique + recherche (utilisé par Orders/History)
    Task<ServiceResult<List<OrderHistoryItem>>> ListAsync(string? query = null);

    public record OrderCreatedInfo(
        string OrderId,
        decimal Total,
        string SupplierName,
        string ProductName
    );

    public record OrderHistoryItem(
        string Id,
        string SupplierId,
        string SupplierName,
        string ProductName,
        int Quantity,
        DateTime CreatedAt,
        decimal Total
    );
}