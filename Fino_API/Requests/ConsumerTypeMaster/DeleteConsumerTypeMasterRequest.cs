using FluentValidation;
namespace FintechApp.Requests.ConsumerTypeMaster { public class DeleteConsumerTypeMasterRequest { public int ConsumerTypeMasterID { get; set; } } public class DeleteConsumerTypeMasterRequestValidator : AbstractValidator<DeleteConsumerTypeMasterRequest> { public DeleteConsumerTypeMasterRequestValidator() { RuleFor(x => x.ConsumerTypeMasterID).GreaterThan(0); } } }
