using FluentValidation;

namespace FintechApp.Requests.MessageSetting
{
    public class UpdateMessageSettingRequest
    {
        public int MessageSettingID { get; set; }
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

    public class UpdateMessageSettingRequestValidator : AbstractValidator<UpdateMessageSettingRequest>
    {
        public UpdateMessageSettingRequestValidator()
        {
            RuleFor(x => x.MessageSettingID).GreaterThan(0);
            RuleFor(x => x.Type).NotEmpty().MaximumLength(100);
        }
    }
}
