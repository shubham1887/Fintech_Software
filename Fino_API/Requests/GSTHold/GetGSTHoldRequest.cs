using FluentValidation;
namespace FintechApp.Requests.GSTHold
{
    public class GetGSTHoldRequest { public int GSTHoldID { get; set; }
    public int UserID { get; set; } }
    public class GetGSTHoldRequestValidator : AbstractValidator<GetGSTHoldRequest> { public GetGSTHoldRequestValidator() { RuleFor(x => x.GSTHoldID).GreaterThan(0); } }
}
