using FluentValidation;
namespace FintechApp.Requests.UserwiseSaleTarget { public class DeleteUserwiseSaleTargetRequest { public int UserwiseSaleTargetID { get; set; } } public class DeleteUserwiseSaleTargetRequestValidator : AbstractValidator<DeleteUserwiseSaleTargetRequest> { public DeleteUserwiseSaleTargetRequestValidator() { RuleFor(x => x.UserwiseSaleTargetID).GreaterThan(0); } } }
