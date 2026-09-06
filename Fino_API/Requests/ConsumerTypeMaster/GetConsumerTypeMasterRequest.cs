using FluentValidation;
namespace FintechApp.Requests.ConsumerTypeMaster { public class GetConsumerTypeMasterRequest { public int ConsumerTypeMasterID { get; set; } } public class GetConsumerTypeMasterRequestValidator : AbstractValidator<GetConsumerTypeMasterRequest> { public GetConsumerTypeMasterRequestValidator() { RuleFor(x => x.ConsumerTypeMasterID).GreaterThan(0); } } }
