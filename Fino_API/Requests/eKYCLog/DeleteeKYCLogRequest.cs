using FluentValidation;
namespace FintechApp.Requests.eKYCLog { public class DeleteeKYCLogRequest { public int eKYCLogID { get; set; } } public class DeleteeKYCLogRequestValidator : AbstractValidator<DeleteeKYCLogRequest> { public DeleteeKYCLogRequestValidator() { RuleFor(x => x.eKYCLogID).GreaterThan(0); } } }
