namespace Application.Features.Products.DeleteProduct;

using FluentValidation;

public sealed class DeleteProductCommandValidator
    : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}