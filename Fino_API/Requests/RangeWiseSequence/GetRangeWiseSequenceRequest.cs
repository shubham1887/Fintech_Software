using FluentValidation;
namespace FintechApp.Requests.RangeWiseSequence
{
    public class GetRangeWiseSequenceRequest { public int RangeWiseSequenceID { get; set; } public int OperatorID { get; set; } }
    public class GetRangeWiseSequenceRequestValidator : AbstractValidator<GetRangeWiseSequenceRequest> { public GetRangeWiseSequenceRequestValidator() { RuleFor(x => x.RangeWiseSequenceID).GreaterThan(0); } }
}
