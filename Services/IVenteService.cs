using System.Threading.Tasks;

namespace project_pharmacie.Services;

public interface IVenteService
{
    Task<ServiceResult<SaleCreatedInfo>> CreateAsync(
        string clientId,
        string productRef,
        int quantity,
        bool createInvoice
    );

    public record SaleCreatedInfo(
        string SaleId,
        decimal Total,
        string ClientName,
        string ProductName,
        int Quantity,
        bool InvoiceCreated,
        int PointsAdded
    );
}