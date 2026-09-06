using FluentValidation;
namespace FintechApp.Requests.ProviderCallBackLog
{
    public class CreateProviderCallBackLogRequest { public string ProviderReply { get; set; } = string.Empty; public int ProviderID { get; set; } public string Type { get; set; } = string.Empty; public int TransactionMasterID { get; set; } }
    public class CreateProviderCallBackLogRequestValidator : AbstractValidator<CreateProviderCallBackLogRequest> { public CreateProviderCallBackLogRequestValidator() {  } }
}
