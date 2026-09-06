using FluentValidation;
namespace FintechApp.Requests.RangeWiseSequence
{
    public class UpdateRangeWiseSequenceRequest { public int RangeWiseSequenceID { get; set; } public int ProviderID { get; set; } public int ServiceID { get; set; } public int OperatorID { get; set; } public decimal FromAmount { get; set; } public decimal ToAmount { get; set; } public bool IsDelete { get; set; } }
    public class UpdateRangeWiseSequenceRequestValidator : AbstractValidator<UpdateRangeWiseSequenceRequest> { public UpdateRangeWiseSequenceRequestValidator() { RuleFor(x => x.RangeWiseSequenceID).GreaterThan(0); } }
}
