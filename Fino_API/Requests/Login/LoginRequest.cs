using FluentValidation;

namespace FintechApp.Requests.Login
{
    public class LoginRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty; // Web, Android, API
        public string IPAddress { get; set; } = string.Empty;
    }
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        private static readonly string[] AllowedDeviceTypes = { "web", "android", "api" };

        public LoginRequestValidator()
        {
            // MobileNumber
            RuleFor(x => x.MobileNumber)
                .NotEmpty().WithMessage(FintechApp.Messages.UserMessages.RequiredField) 
                .Length(10).WithMessage(FintechApp.Messages.UserMessages.InvalidLength)
                .Matches(@"^\d{10}$").WithMessage(FintechApp.Messages.UserMessages.InvalidMobileNumber); 

            // Password
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(FintechApp.Messages.UserMessages.RequiredField) 
                .MinimumLength(6).WithMessage(FintechApp.Messages.UserMessages.InvalidLength) 
                .MaximumLength(100).WithMessage(FintechApp.Messages.UserMessages.InvalidLength); 

            // DeviceType
            RuleFor(x => x.DeviceType)
                .NotEmpty().WithMessage(FintechApp.Messages.UserMessages.RequiredField) 
                .Must(d => AllowedDeviceTypes.Contains(d.ToLower()))
                .WithMessage(FintechApp.Messages.UserMessages.InvalidValue); 

            // IPAddress
            RuleFor(x => x.IPAddress)
                .NotEmpty().WithMessage(FintechApp.Messages.UserMessages.RequiredField) 
                .Matches(@"^(\d{1,3}\.){3}\d{1,3}$")
                .WithMessage(FintechApp.Messages.UserMessages.InvalidFormat); 
        }
    }
}