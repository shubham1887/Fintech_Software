using FluentValidation;
namespace FintechApp.Requests.OperatorSequence
{
    public class CreateOperatorSequenceRequest { public int ServiceID { get; set; } public int OperatorID { get; set; } public int ProviderID { get; set; } public int Count { get; set; } }
    public class CreateOperatorSequenceRequestValidator : AbstractValidator<CreateOperatorSequenceRequest> { public CreateOperatorSequenceRequestValidator() { RuleFor(x => x.ServiceID).GreaterThan(0); RuleFor(x => x.OperatorID).GreaterThan(0); } }
}
