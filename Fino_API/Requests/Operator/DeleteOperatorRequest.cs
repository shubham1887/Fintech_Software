using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.Operator
{
    public class DeleteOperatorRequest
    {
        public int OperatorID { get; set; }
    }
    public class DeleteOperatorRequestValidator : AbstractValidator<DeleteOperatorRequest>
    {
        public DeleteOperatorRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                .GreaterThan(0).WithMessage(OperatorMessages.InvalidValue);
        }
    }
}
