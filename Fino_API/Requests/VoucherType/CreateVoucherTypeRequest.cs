using FluentValidation;
namespace FintechApp.Requests.VoucherType
{
    public class CreateVoucherTypeRequest { public string? VoucherTypeName { get; set; } }
    public class CreateVoucherTypeRequestValidator : AbstractValidator<CreateVoucherTypeRequest> { public CreateVoucherTypeRequestValidator() { } }
}
