using FluentValidation;

namespace FintechApp.Requests.SliderMaster
{
    public class CreateSliderMasterRequest
    {
        public string SliderPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class CreateSliderMasterRequestValidator : AbstractValidator<CreateSliderMasterRequest>
    {
        public CreateSliderMasterRequestValidator()
        {
            RuleFor(x => x.SliderPath).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
        }
    }
}
