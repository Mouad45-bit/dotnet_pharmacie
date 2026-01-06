namespace project_pharmacie.Services;

public record SupplierRowVm(
    string Id,
    string Name,
    string Phone,
    double Rating,
    int RatingsCount,
    bool IsPreferred
);

public record SupplierListVm(
    List<SupplierRowVm> Rows,
    int TotalSuppliers,
    double AverageRating,
    int TotalRatingsCount,
    string BestSupplierName,
    double BestSupplierRating
);

public record SupplierRateVm(
    string SupplierId,
    string SupplierName,
    double CurrentRating,
    int CurrentRatingsCount
);