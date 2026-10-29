namespace Application.Features.Products.CreateProduct;

public sealed record CreateProductResponse(
    Guid Id,
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt
);