using FluentValidation;
namespace FintechApp.Requests.OperatorSequenceUserWise
{
    public class CreateOperatorSequenceUserWiseRequest { public int UserID { get; set; } public int ServiceID { get; set; } public int OperatorID { get; set; } public int ProviderID { get; set; } }
    public class CreateOperatorSequenceUserWiseRequestValidator : AbstractValidator<CreateOperatorSequenceUserWiseRequest> { public CreateOperatorSequenceUserWiseRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.ServiceID).GreaterThan(0); RuleFor(x => x.OperatorID).GreaterThan(0); RuleFor(x => x.ProviderID).GreaterThan(0); } }
}
