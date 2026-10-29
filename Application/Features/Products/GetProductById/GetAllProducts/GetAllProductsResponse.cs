namespace Application.Features.Products.GetAllProducts;

public sealed record GetAllProductsResponse(
    List<ProductResponse> Products
);

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt
);