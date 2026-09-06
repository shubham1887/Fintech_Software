using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.DMTSender
{
    public class UpdateDMTSenderRequest
    {
        public int SenderID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class UpdateDMTSenderRequestValidator : AbstractValidator<UpdateDMTSenderRequest>
    {
        public UpdateDMTSenderRequestValidator()
        {
            RuleFor(x => x.SenderID)
                .GreaterThan(0).WithMessage(DMTSenderMessages.InvalidValue);

            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(DMTSenderMessages.RequiredField);

            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(DMTSenderMessages.RequiredField);
        }
    }
}
