// Path: BlogHybrid.Application/Validators/Auth/RegisterUserCommandValidator.cs
using BlogHybrid.Application.Commands.Auth;
using FluentValidation;

namespace BlogHybrid.Application.Validators.Auth
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("อีเมลเป็นข้อมูลที่จำเป็น")
                .EmailAddress().WithMessage("รูปแบบอีเมลไม่ถูกต้อง")
                .MaximumLength(256).WithMessage("อีเมลต้องไม่เกิน 256 ตัวอักษร");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("รหัสผ่านเป็นข้อมูลที่จำเป็น")
                .MinimumLength(8).WithMessage("รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร")
                .Matches(@"[A-Z]").WithMessage("รหัสผ่านต้องมีตัวพิมพ์ใหญ่อย่างน้อย 1 ตัว")
                .Matches(@"[a-z]").WithMessage("รหัสผ่านต้องมีตัวพิมพ์เล็กอย่างน้อย 1 ตัว")
                .Matches(@"[0-9]").WithMessage("รหัสผ่านต้องมีตัวเลขอย่างน้อย 1 ตัว")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("รหัสผ่านต้องมีอักขระพิเศษอย่างน้อย 1 ตัว");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("กรุณายืนยันรหัสผ่าน")
                .Equal(x => x.Password).WithMessage("รหัสผ่านไม่ตรงกัน");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("ชื่อที่แสดงเป็นข้อมูลที่จำเป็น")
                .MinimumLength(2).WithMessage("ชื่อที่แสดงต้องมีอย่างน้อย 2 ตัวอักษร")
                .MaximumLength(100).WithMessage("ชื่อที่แสดงต้องไม่เกิน 100 ตัวอักษร");

            RuleFor(x => x.FirstName)
                .MaximumLength(50).WithMessage("ชื่อจริงต้องไม่เกิน 50 ตัวอักษร")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(50).WithMessage("นามสกุลต้องไม่เกิน 50 ตัวอักษร")
                .When(x => !string.IsNullOrEmpty(x.LastName));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^[0-9\-\+\(\)\s]*$").WithMessage("หมายเลขโทรศัพท์ไม่ถูกต้อง")
                .MaximumLength(20).WithMessage("หมายเลขโทรศัพท์ต้องไม่เกิน 20 ตัวอักษร")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.AcceptTerms)
                .Equal(true).WithMessage("กรุณายอมรับข้อกำหนดและเงื่อนไข");
        }
    }
}