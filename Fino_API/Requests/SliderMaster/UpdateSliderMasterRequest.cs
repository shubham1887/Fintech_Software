using FluentValidation;

namespace FintechApp.Requests.SliderMaster
{
    public class UpdateSliderMasterRequest
    {
        public int SliderMasterID { get; set; }
        public string SliderPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class UpdateSliderMasterRequestValidator : AbstractValidator<UpdateSliderMasterRequest>
    {
        public UpdateSliderMasterRequestValidator()
        {
            RuleFor(x => x.SliderMasterID).GreaterThan(0);
            RuleFor(x => x.SliderPath).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
        }
    }
}
