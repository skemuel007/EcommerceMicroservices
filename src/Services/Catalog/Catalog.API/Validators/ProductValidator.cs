using Catalog.API.Dtos.Request;
using FluentValidation;

namespace Catalog.API.Validators
{
    public class ProductValidator : AbstractValidator<ProductDto>
    {
        public ProductValidator()
        {
            RuleFor(p => p.Name).NotEmpty();
            RuleFor(p => p.Price).NotEmpty();
            RuleFor(p => p.Category).NotEmpty();
            RuleFor(p => p.Summary).NotEmpty();
        }
    }
}
