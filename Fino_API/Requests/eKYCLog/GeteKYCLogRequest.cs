using FluentValidation;
namespace FintechApp.Requests.eKYCLog { public class GeteKYCLogRequest { public int eKYCLogID { get; set; } public int UserID { get; set; } } public class GeteKYCLogRequestValidator : AbstractValidator<GeteKYCLogRequest> { public GeteKYCLogRequestValidator() { RuleFor(x => x.eKYCLogID).GreaterThan(0); } } }
