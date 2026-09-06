using FluentValidation;
namespace FintechApp.Requests.RangeWiseSequence
{
    public class DeleteRangeWiseSequenceRequest { public int RangeWiseSequenceID { get; set; } }
    public class DeleteRangeWiseSequenceRequestValidator : AbstractValidator<DeleteRangeWiseSequenceRequest> { public DeleteRangeWiseSequenceRequestValidator() { RuleFor(x => x.RangeWiseSequenceID).GreaterThan(0); } }
}
