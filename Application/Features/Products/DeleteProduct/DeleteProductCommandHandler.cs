namespace Application.Features.Products.DeleteProduct;

using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteProductCommandHandler
    : ICommandHandler<DeleteProductCommand, Result<DeleteProductResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IValidator<DeleteProductCommand> _validator;

    public DeleteProductCommandHandler(
        IAppDbContext dbContext,
        IValidator<DeleteProductCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<Result<DeleteProductResponse>> HandleAsync(
        DeleteProductCommand command,
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

            return Result<DeleteProductResponse>.Failure(
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
            return Result<DeleteProductResponse>.Failure(
                Error.NotFound(
                    "Product.NotFound",
                    $"Product with ID '{command.Id}' was not found."));
        }

        _dbContext.Products.Remove(product);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<DeleteProductResponse>.Success(
            new DeleteProductResponse(product.Id));
    }
}