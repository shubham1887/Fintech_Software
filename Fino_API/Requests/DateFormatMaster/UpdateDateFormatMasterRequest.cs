using FluentValidation;
namespace FintechApp.Requests.DateFormatMaster
{
    public class UpdateDateFormatMasterRequest { public int DateFormatMasterkID { get; set; } public string DateFormat { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class UpdateDateFormatMasterRequestValidator : AbstractValidator<UpdateDateFormatMasterRequest> { public UpdateDateFormatMasterRequestValidator() { RuleFor(x => x.DateFormatMasterkID).GreaterThan(0); } }
}
