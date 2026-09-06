using FluentValidation;

namespace FintechApp.Requests.EmailSetting
{
    public class CreateEmailSettingRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool IsSSL { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateEmailSettingRequestValidator : AbstractValidator<CreateEmailSettingRequest>
    {
        public CreateEmailSettingRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
            RuleFor(x => x.Password).NotEmpty().MaximumLength(100);
            RuleFor(x => x.HostName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Port).GreaterThan(0);
        }
    }
}
