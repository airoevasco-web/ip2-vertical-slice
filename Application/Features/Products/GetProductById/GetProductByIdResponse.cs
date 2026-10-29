namespace Application.Features.Products.GetProductById;

public sealed record GetProductByIdResponse(
    Guid Id,
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt
);