using FluentValidation;

namespace FintechApp.Requests.MessageSetting
{
    public class CreateMessageSettingRequest
    {
        public string Type { get; set; } = string.Empty;
        public bool IsSMS { get; set; }
        public bool IsMail { get; set; }
        public bool IsWhatsUp { get; set; }
        public string WhatsUpMessage { get; set; } = string.Empty;
        public string SmsMessage { get; set; } = string.Empty;
        public string EmailMessage { get; set; } = string.Empty;
        public string TemplateID { get; set; } = string.Empty;
        public bool IsOTP { get; set; }
    }

    public class CreateMessageSettingRequestValidator : AbstractValidator<CreateMessageSettingRequest>
    {
        public CreateMessageSettingRequestValidator()
        {
            RuleFor(x => x.Type).NotEmpty().MaximumLength(100);
        }
    }
}
