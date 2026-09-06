using FluentValidation;
namespace FintechApp.Requests.ProviderCallBackLog
{
    public class UpdateProviderCallBackLogRequest { public int ProviderCallBackID { get; set; } public string ProviderReply { get; set; } = string.Empty; public int ProviderID { get; set; } public string Type { get; set; } = string.Empty; public int TransactionMasterID { get; set; } }
    public class UpdateProviderCallBackLogRequestValidator : AbstractValidator<UpdateProviderCallBackLogRequest> { public UpdateProviderCallBackLogRequestValidator() { RuleFor(x => x.ProviderCallBackID).GreaterThan(0); } }
}
