namespace Application.Features.Products.GetProductById;

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

public sealed class GetProductByIdQueryHandler
    : IQueryHandler<GetProductByIdQuery, Result<GetProductByIdResponse>>
{
    private readonly IAppDbContext _dbContext;

    public GetProductByIdQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetProductByIdResponse>> HandleAsync(
        GetProductByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);

        if (product is null)
        {
            return Result<GetProductByIdResponse>.Failure(
                Error.NotFound(
                    "Product.NotFound",
                    $"Product with ID '{query.Id}' was not found."));
        }

        var response = new GetProductByIdResponse(
            product.Id,
            product.Name,
            product.Sku,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.CreatedAt);

        return Result<GetProductByIdResponse>.Success(response);
    }
}