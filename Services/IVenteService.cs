using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace project_pharmacie.Services;

public interface IVenteService
{
    // Création vente
    Task<ServiceResult<SaleCreatedInfo>> CreateAsync(
        string clientId,
        string productRef,
        int quantity,
        bool createInvoice
    );

    // Historique + recherche (utilisé par Sales/History)
    Task<ServiceResult<List<SaleHistoryItem>>> ListAsync(string? query = null);

    public record SaleCreatedInfo(
        string SaleId,
        decimal Total,
        string ClientName,
        string ProductName,
        int Quantity,
        bool InvoiceCreated,
        int PointsAdded
    );

    public record SaleHistoryItem(
        string Id,
        string CustomerName,
        string DrugName,
        int Quantity,
        decimal TotalPrice,
        string Status,
        DateTime Date
    );
}