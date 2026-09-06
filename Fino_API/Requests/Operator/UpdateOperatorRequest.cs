using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.Operator
{
    public class UpdateOperatorRequest
    {
        public int OperatorID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class UpdateOperatorRequestValidator : AbstractValidator<UpdateOperatorRequest>
    {
        public UpdateOperatorRequestValidator()
        {
            // OperatorID
            RuleFor(x => x.OperatorID)
                .GreaterThan(0).WithMessage(OperatorMessages.InvalidValue);

            // FieldName
            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(OperatorMessages.RequiredField)
                .MaximumLength(100).WithMessage(OperatorMessages.InvalidLength);

            // OldValue
            RuleFor(x => x.OldValue)
                .NotEmpty().WithMessage(OperatorMessages.RequiredField)
                .MaximumLength(500).WithMessage(OperatorMessages.InvalidLength);

            // NewValue
            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(OperatorMessages.RequiredField)
                .MaximumLength(500).WithMessage(OperatorMessages.InvalidLength);
        }
    }
}