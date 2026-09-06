using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.DMTSender
{
    public class CreateDMTSenderRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string GUID { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int UserID { get; set; }
        public bool IsActive { get; set; } = true;
        public string IPAddress { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
    }

    public class CreateDMTSenderRequestValidator : AbstractValidator<CreateDMTSenderRequest>
    {
        public CreateDMTSenderRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(DMTSenderMessages.InvalidName)
                .MaximumLength(200).WithMessage(DMTSenderMessages.InvalidLength);

            RuleFor(x => x.Mobile)
                .NotEmpty().WithMessage(DMTSenderMessages.InvalidMobile)
                .Length(10).WithMessage(DMTSenderMessages.InvalidMobile)
                .Matches("^[0-9]+$").WithMessage(DMTSenderMessages.InvalidMobile);

            RuleFor(x => x.GUID)
                .NotEmpty().WithMessage(DMTSenderMessages.RequiredField)
                .MaximumLength(50).WithMessage(DMTSenderMessages.InvalidLength);

            RuleFor(x => x.UserID)
                .GreaterThan(0).WithMessage(DMTSenderMessages.InvalidUserID);

            RuleFor(x => x.Pincode)
                .MaximumLength(10).WithMessage(DMTSenderMessages.InvalidLength);
        }
    }
}
