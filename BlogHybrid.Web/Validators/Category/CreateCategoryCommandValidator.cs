using BlogHybrid.Application.Commands.Category;
using FluentValidation;

namespace BlogHybrid.Web.Validators.Category
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .Length(1, 100).WithMessage("Category name must be between 1 and 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.ImageUrl)
                .Must(BeValidUrl).WithMessage("Invalid URL format")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("Color is required")
                .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Invalid color format");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order must be non-negative")
                .LessThanOrEqualTo(999).WithMessage("Sort order must not exceed 999");
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }

    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid category ID");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .Length(1, 100).WithMessage("Category name must be between 1 and 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.ImageUrl)
                .Must(BeValidUrl).WithMessage("Invalid URL format")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("Color is required")
                .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Invalid color format");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order must be non-negative")
                .LessThanOrEqualTo(999).WithMessage("Sort order must not exceed 999");
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
