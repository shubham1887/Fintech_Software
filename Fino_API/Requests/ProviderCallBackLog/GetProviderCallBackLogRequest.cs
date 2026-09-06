using FluentValidation;
namespace FintechApp.Requests.ProviderCallBackLog
{
    public class GetProviderCallBackLogRequest { public int ProviderCallBackID { get; set; } public int TransactionID { get; set; } }
    public class GetProviderCallBackLogRequestValidator : AbstractValidator<GetProviderCallBackLogRequest> { public GetProviderCallBackLogRequestValidator() { RuleFor(x => x.ProviderCallBackID).GreaterThan(0); } }
}
