using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.Service
{
    public class UpdateServiceRequest
    {
        public int ServiceID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class UpdateServiceRequestValidator : AbstractValidator<UpdateServiceRequest>
    {
        public UpdateServiceRequestValidator()
        {
            // ServiceID
            RuleFor(x => x.ServiceID)
                .GreaterThan(0).WithMessage(ServiceMessages.InvalidValue);

            // FieldName
            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(ServiceMessages.RequiredField)
                .MaximumLength(100).WithMessage(ServiceMessages.InvalidLength);

            // OldValue
            RuleFor(x => x.OldValue)
                .NotEmpty().WithMessage(ServiceMessages.RequiredField)
                .MaximumLength(500).WithMessage(ServiceMessages.InvalidLength);

            // NewValue
            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(ServiceMessages.RequiredField)
                .MaximumLength(500).WithMessage(ServiceMessages.InvalidLength);
        }
    }
}