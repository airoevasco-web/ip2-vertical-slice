namespace Application.Features.Products.UpdateProduct;

using FluentValidation;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0);
    }
}