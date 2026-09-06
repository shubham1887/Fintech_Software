using FluentValidation;
namespace FintechApp.Requests.GSTHold
{
    public class DeleteGSTHoldRequest { public int GSTHoldID { get; set; } }
    public class DeleteGSTHoldRequestValidator : AbstractValidator<DeleteGSTHoldRequest> { public DeleteGSTHoldRequestValidator() { RuleFor(x => x.GSTHoldID).GreaterThan(0); } }
}
