namespace Application.Features.Products.CreateProduct;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record CreateProductCommand(
    string Name,
    string Sku,
    string? Description,
    decimal Price,
    int StockQuantity
) : ICommand<Result<CreateProductResponse>>;