namespace Application.Features.Products.UpdateProduct;

public sealed record UpdateProductResponse(
    Guid Id,
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt
);