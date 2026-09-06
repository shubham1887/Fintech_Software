using FluentValidation;

namespace FintechApp.Requests.SlabMaster
{
    public class CreateSlabMasterRequest
    {
        public string SlabName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class CreateSlabMasterRequestValidator : AbstractValidator<CreateSlabMasterRequest>
    {
        public CreateSlabMasterRequestValidator()
        {
            RuleFor(x => x.SlabName).NotEmpty().MaximumLength(80);
        }
    }
}
