namespace Application.Features.Products.DeleteProduct;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record DeleteProductCommand(
    Guid Id
) : ICommand<Result<DeleteProductResponse>>;