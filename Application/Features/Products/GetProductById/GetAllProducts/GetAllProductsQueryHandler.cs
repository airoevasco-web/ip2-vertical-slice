namespace Application.Features.Products.GetAllProducts;

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

public sealed class GetAllProductsQueryHandler
    : IQueryHandler<GetAllProductsQuery, Result<GetAllProductsResponse>>
{
    private readonly IAppDbContext _dbContext;

    public GetAllProductsQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetAllProductsResponse>> HandleAsync(
        GetAllProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.Products
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Sku,
                p.Description,
                p.Price,
                p.StockQuantity,
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        var response = new GetAllProductsResponse(products);

        return Result<GetAllProductsResponse>.Success(response);
    }
}