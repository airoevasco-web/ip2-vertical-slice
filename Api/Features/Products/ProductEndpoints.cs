namespace Api.Features.Products;

using Api.Common.Extensions;
using Application.Abstractions.Messaging;
using Application.Features.Products.CreateProduct;
using Application.Features.Products.DeleteProduct;
using Application.Features.Products.GetAllProducts;
using Application.Features.Products.GetProductById;
using Application.Features.Products.UpdateProduct;
using Domain.Common;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapPost("/", async (
            CreateProductCommand command,
            ICommandHandler<CreateProductCommand, Result<CreateProductResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.ToProblemDetails();
            }

            return TypedResults.Created(
                $"/api/products/{result.Value!.Id}",
                result.Value);
        })
        .WithName("CreateProduct")
        .WithSummary("Create a new product");

        group.MapGet("/", async (
            IQueryHandler<GetAllProductsQuery, Result<GetAllProductsResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new GetAllProductsQuery(),
                cancellationToken);

            if (!result.IsSuccess)
            {
                return result.ToProblemDetails();
            }

            return TypedResults.Ok(result.Value);
        })
        .WithName("GetAllProducts")
        .WithSummary("Get all products");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IQueryHandler<GetProductByIdQuery, Result<GetProductByIdResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new GetProductByIdQuery(id),
                cancellationToken);

            if (!result.IsSuccess)
            {
                return result.ToProblemDetails();
            }

            return TypedResults.Ok(result.Value);
        })
        .WithName("GetProductById")
        .WithSummary("Get a product by ID");

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateProductRequest request,
            ICommandHandler<UpdateProductCommand, Result<UpdateProductResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateProductCommand(
                id,
                request.Name,
                request.Sku,
                request.Description,
                request.Price,
                request.StockQuantity);

            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.ToProblemDetails();
            }

            return TypedResults.Ok(result.Value);
        })
        .WithName("UpdateProduct")
        .WithSummary("Update a product");

        group.MapDelete("/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteProductCommand, Result<DeleteProductResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new DeleteProductCommand(id),
                cancellationToken);

            if (!result.IsSuccess)
            {
                return result.ToProblemDetails();
            }

            return TypedResults.NoContent();
        })
        .WithName("DeleteProduct")
        .WithSummary("Delete a product");
    }

    private sealed record UpdateProductRequest(
        string Name,
        string Sku,
        string? Description,
        decimal Price,
        int StockQuantity);
}