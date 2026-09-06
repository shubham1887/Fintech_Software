using FluentValidation;
namespace FintechApp.Requests.RangeWiseSequence
{
    public class CreateRangeWiseSequenceRequest { public int ProviderID { get; set; } public int ServiceID { get; set; } public int OperatorID { get; set; } public decimal FromAmount { get; set; } public decimal ToAmount { get; set; } public bool IsDelete { get; set; } }
    public class CreateRangeWiseSequenceRequestValidator : AbstractValidator<CreateRangeWiseSequenceRequest> { public CreateRangeWiseSequenceRequestValidator() { RuleFor(x => x.ProviderID).GreaterThan(0); RuleFor(x => x.OperatorID).GreaterThan(0); } }
}

