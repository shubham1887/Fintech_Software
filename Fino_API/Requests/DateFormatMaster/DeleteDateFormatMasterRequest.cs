using FluentValidation;
namespace FintechApp.Requests.DateFormatMaster
{
    public class DeleteDateFormatMasterRequest { public int DateFormatMasterkID { get; set; } }
    public class DeleteDateFormatMasterRequestValidator : AbstractValidator<DeleteDateFormatMasterRequest> { public DeleteDateFormatMasterRequestValidator() { RuleFor(x => x.DateFormatMasterkID).GreaterThan(0); } }
}
