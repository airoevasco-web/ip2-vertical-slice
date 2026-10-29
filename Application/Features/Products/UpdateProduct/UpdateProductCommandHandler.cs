namespace Application.Features.Products.UpdateProduct;

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateProductCommandHandler
    : ICommandHandler<UpdateProductCommand, Result<UpdateProductResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IValidator<UpdateProductCommand> _validator;

    public UpdateProductCommandHandler(
        IAppDbContext dbContext,
        IValidator<UpdateProductCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<Result<UpdateProductResponse>> HandleAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(
            command,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            var message = string.Join(
                "; ",
                validationResult.Errors.Select(error => error.ErrorMessage));

            return Result<UpdateProductResponse>.Failure(
                Error.Validation(
                    "Product.Validation",
                    message));
        }

        var product = await _dbContext.Products
            .FirstOrDefaultAsync(
                p => p.Id == command.Id,
                cancellationToken);

        if (product is null)
        {
            return Result<UpdateProductResponse>.Failure(
                Error.NotFound(
                    "Product.NotFound",
                    $"Product with ID '{command.Id}' was not found."));
        }

        var skuExists = await _dbContext.Products
            .AnyAsync(
                p => p.Sku == command.Sku && p.Id != command.Id,
                cancellationToken);

        if (skuExists)
        {
            return Result<UpdateProductResponse>.Failure(
                new Error(
                    "Product.SkuConflict",
                    $"A product with SKU '{command.Sku}' already exists."));
        }

        product.Name = command.Name;
        product.Sku = command.Sku;
        product.Description = command.Description;
        product.Price = command.Price;
        product.StockQuantity = command.StockQuantity;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new UpdateProductResponse(
            product.Id,
            product.Name,
            product.Sku,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.CreatedAt);

        return Result<UpdateProductResponse>.Success(response);
    }
}