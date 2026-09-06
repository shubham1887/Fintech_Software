using FluentValidation;
namespace FintechApp.Requests.DateFormatMaster
{
    public class GetDateFormatMasterRequest { public int DateFormatMasterkID { get; set; } }
    public class GetDateFormatMasterRequestValidator : AbstractValidator<GetDateFormatMasterRequest> { public GetDateFormatMasterRequestValidator() { RuleFor(x => x.DateFormatMasterkID).GreaterThan(0); } }
}
