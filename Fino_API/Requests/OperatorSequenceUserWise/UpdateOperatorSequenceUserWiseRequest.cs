using FluentValidation;
namespace FintechApp.Requests.OperatorSequenceUserWise
{
    public class UpdateOperatorSequenceUserWiseRequest { public int OperatorSequenceUserWiseID { get; set; } public int UserID { get; set; } public int ServiceID { get; set; } public int OperatorID { get; set; } public int ProviderID { get; set; } }
    public class UpdateOperatorSequenceUserWiseRequestValidator : AbstractValidator<UpdateOperatorSequenceUserWiseRequest> { public UpdateOperatorSequenceUserWiseRequestValidator() { RuleFor(x => x.OperatorSequenceUserWiseID).GreaterThan(0); } }
}
