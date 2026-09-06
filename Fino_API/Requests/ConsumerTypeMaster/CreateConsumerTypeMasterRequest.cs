using FluentValidation;
namespace FintechApp.Requests.ConsumerTypeMaster { public class CreateConsumerTypeMasterRequest { public string ConsumerType { get; set; } = string.Empty; } public class CreateConsumerTypeMasterRequestValidator : AbstractValidator<CreateConsumerTypeMasterRequest> { public CreateConsumerTypeMasterRequestValidator() {  } } }
