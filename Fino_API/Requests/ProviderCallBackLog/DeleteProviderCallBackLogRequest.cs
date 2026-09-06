using FluentValidation;
namespace FintechApp.Requests.ProviderCallBackLog
{
    public class DeleteProviderCallBackLogRequest { public int ProviderCallBackID { get; set; } }
    public class DeleteProviderCallBackLogRequestValidator : AbstractValidator<DeleteProviderCallBackLogRequest> { public DeleteProviderCallBackLogRequestValidator() { RuleFor(x => x.ProviderCallBackID).GreaterThan(0); } }
}
