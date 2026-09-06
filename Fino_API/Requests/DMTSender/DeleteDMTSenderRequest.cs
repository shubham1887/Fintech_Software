using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.DMTSender
{
    public class DeleteDMTSenderRequest
    {
        public int SenderID { get; set; }
    }

    public class DeleteDMTSenderRequestValidator : AbstractValidator<DeleteDMTSenderRequest>
    {
        public DeleteDMTSenderRequestValidator()
        {
            RuleFor(x => x.SenderID)
                .GreaterThan(0).WithMessage(DMTSenderMessages.InvalidValue);
        }
    }
}
