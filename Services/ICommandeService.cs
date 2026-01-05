namespace project_pharmacie.Services;

public interface ICommandeService
{
    Task<ServiceResult<OrderCreatedInfo>> CreateAsync(string supplierId, string productRef, int quantity);

    public record OrderCreatedInfo(
        string OrderId,
        decimal Total,
        string SupplierName,
        string ProductName
    );
}