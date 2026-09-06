using FluentValidation;
namespace FintechApp.Requests.DateFormatMaster
{
    public class CreateDateFormatMasterRequest { public string DateFormat { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class CreateDateFormatMasterRequestValidator : AbstractValidator<CreateDateFormatMasterRequest> { public CreateDateFormatMasterRequestValidator() {  } }
}
