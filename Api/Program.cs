using Api.Features.Products;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Features.Products.CreateProduct;
using Application.Features.Products.DeleteProduct;
using Application.Features.Products.GetAllProducts;
using Application.Features.Products.GetProductById;
using Application.Features.Products.UpdateProduct;
using Domain.Common;
using FluentValidation;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<IAppDbContext, AppDbContext>(options =>
    options.UseInMemoryDatabase("VerticalSliceDb"));

// Validators
builder.Services.AddValidatorsFromAssembly(typeof(IAppDbContext).Assembly);

// Handlers
builder.Services.AddScoped<
    ICommandHandler<CreateProductCommand, Result<CreateProductResponse>>,
    CreateProductCommandHandler>();

builder.Services.AddScoped<
    IQueryHandler<GetProductByIdQuery, Result<GetProductByIdResponse>>,
    GetProductByIdQueryHandler>();

builder.Services.AddScoped<
    IQueryHandler<GetAllProductsQuery, Result<GetAllProductsResponse>>,
    GetAllProductsQueryHandler>();

builder.Services.AddScoped<
    ICommandHandler<UpdateProductCommand, Result<UpdateProductResponse>>,
    UpdateProductCommandHandler>();

builder.Services.AddScoped<
    ICommandHandler<DeleteProductCommand, Result<DeleteProductResponse>>,
    DeleteProductCommandHandler>();

// API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Vertical Slice API Template by Elvin Manuel R. Luces, MIT";
        document.Info.Description = "A simple API demonstrating Vertical Slice Architecture";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    dbContext.Products.AddRange(
        new Domain.Entities.Product
        {
            Name = "Wireless Mouse",
            Sku = "SKU-MOU-001",
            Description = "Ergonomic 2.4GHz wireless mouse",
            Price = 799.00m,
            StockQuantity = 25,
            CreatedAt = DateTime.UtcNow
        },
        new Domain.Entities.Product
        {
            Name = "Mechanical Keyboard",
            Sku = "SKU-KEY-002",
            Description = "",
            Price = 3499.50m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow
        }
    );

    dbContext.SaveChanges();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Root endpoint
app.MapGet("/", () => "Welcome to the Vertical Slice API! Go to /scalar/v1 for API documentation.");

// Map endpoints
app.MapProductEndpoints();

app.Run();