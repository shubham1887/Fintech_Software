using FluentValidation;

namespace FintechApp.Requests.Mode
{
    public class UpdateModeRequest
    {
        public int ModeID { get; set; }
        public string ModeName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }

    public class UpdateModeRequestValidator : AbstractValidator<UpdateModeRequest>
    {
        public UpdateModeRequestValidator()
        {
            RuleFor(x => x.ModeID).GreaterThan(0);
            RuleFor(x => x.ModeName).NotEmpty().MaximumLength(100);
        }
    }
}
