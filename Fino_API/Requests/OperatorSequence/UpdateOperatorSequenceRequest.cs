using FluentValidation;
namespace FintechApp.Requests.OperatorSequence
{
    public class UpdateOperatorSequenceRequest { public int OperatorSequenceID { get; set; } public int ServiceID { get; set; } public int OperatorID { get; set; } public int ProviderID { get; set; } public int Count { get; set; } }
    public class UpdateOperatorSequenceRequestValidator : AbstractValidator<UpdateOperatorSequenceRequest> { public UpdateOperatorSequenceRequestValidator() { RuleFor(x => x.OperatorSequenceID).GreaterThan(0); } }
}
