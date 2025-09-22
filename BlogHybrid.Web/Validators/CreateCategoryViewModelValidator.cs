using BlogHybrid.Web.Models.ViewModels.Admin;
using FluentValidation;

namespace BlogHybrid.Web.Validators
{
    public class CreateCategoryViewModelValidator : AbstractValidator<CreateCategoryViewModel>
    {
        public CreateCategoryViewModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("กรุณากรอกชื่อหมวดหมู่")
                .Length(1, 100).WithMessage("ชื่อหมวดหมู่ต้องมีความยาว 1-100 ตัวอักษร");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("คำอธิบายต้องมีความยาวไม่เกิน 500 ตัวอักษร");

            RuleFor(x => x.ImageUrl)
                .Must(BeValidUrl).WithMessage("รูปแบบ URL ไม่ถูกต้อง")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("กรุณาเลือกสี")
                .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("รูปแบบสีไม่ถูกต้อง");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("ลำดับการแสดงต้องเป็นตัวเลขที่ไม่น้อยกว่า 0")
                .LessThanOrEqualTo(999).WithMessage("ลำดับการแสดงต้องไม่เกิน 999");
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }

    public class EditCategoryViewModelValidator : AbstractValidator<EditCategoryViewModel>
    {
        public EditCategoryViewModelValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid category ID");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("กรุณากรอกชื่อหมวดหมู่")
                .Length(1, 100).WithMessage("ชื่อหมวดหมู่ต้องมีความยาว 1-100 ตัวอักษร");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("คำอธิบายต้องมีความยาวไม่เกิน 500 ตัวอักษร");

            RuleFor(x => x.ImageUrl)
                .Must(BeValidUrl).WithMessage("รูปแบบ URL ไม่ถูกต้อง")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("กรุณาเลือกสี")
                .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("รูปแบบสีไม่ถูกต้อง");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("ลำดับการแสดงต้องเป็นตัวเลขที่ไม่น้อยกว่า 0")
                .LessThanOrEqualTo(999).WithMessage("ลำดับการแสดงต้องไม่เกิน 999");
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
