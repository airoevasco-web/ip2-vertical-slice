namespace Application.Features.Products.UpdateProduct;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    int StockQuantity
) : ICommand<Result<UpdateProductResponse>>;