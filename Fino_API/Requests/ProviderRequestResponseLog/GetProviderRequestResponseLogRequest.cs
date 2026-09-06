using FluentValidation;
namespace FintechApp.Requests.ProviderRequestResponseLog
{
    public class GetProviderRequestResponseLogRequest { public int ProviderRequestResponseLogID { get; set; }
    public int UserID { get; set; }
    public int TransactionID { get; set; } }
    public class GetProviderRequestResponseLogRequestValidator : AbstractValidator<GetProviderRequestResponseLogRequest> { public GetProviderRequestResponseLogRequestValidator() { RuleFor(x => x.ProviderRequestResponseLogID).GreaterThan(0); } }
}
