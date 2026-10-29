namespace Application.Features.Products.GetProductById;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record GetProductByIdQuery(
    Guid Id
) : IQuery<Result<GetProductByIdResponse>>;