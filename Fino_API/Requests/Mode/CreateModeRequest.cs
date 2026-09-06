using FluentValidation;

namespace FintechApp.Requests.Mode
{
    public class CreateModeRequest
    {
        public string ModeName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }

    public class CreateModeRequestValidator : AbstractValidator<CreateModeRequest>
    {
        public CreateModeRequestValidator()
        {
            RuleFor(x => x.ModeName).NotEmpty().MaximumLength(100);
        }
    }
}
