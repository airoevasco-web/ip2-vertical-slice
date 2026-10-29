namespace Application.Features.Products.CreateProduct;

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public sealed class CreateProductCommandHandler
    : ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IValidator<CreateProductCommand> _validator;

    public CreateProductCommandHandler(
        IAppDbContext dbContext,
        IValidator<CreateProductCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<Result<CreateProductResponse>> HandleAsync(
        CreateProductCommand command,
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

            return Result<CreateProductResponse>.Failure(
                Error.Validation(
                    "Product.Validation",
                    message));
        }

        var skuExists = await _dbContext.Products
            .AnyAsync(p => p.Sku == command.Sku, cancellationToken);

        if (skuExists)
        {
            return Result<CreateProductResponse>.Failure(
                new Error(
                    "Product.SkuConflict",
                    $"A product with SKU '{command.Sku}' already exists."));
        }

        var product = new Product
        {
            Name = command.Name,
            Sku = command.Sku,
            Description = command.Description,
            Price = command.Price,
            StockQuantity = command.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Products.Add(product);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateProductResponse(
            product.Id,
            product.Name,
            product.Sku,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.CreatedAt);

        return Result<CreateProductResponse>.Success(response);
    }
}