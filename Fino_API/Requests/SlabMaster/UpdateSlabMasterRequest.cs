using FluentValidation;

namespace FintechApp.Requests.SlabMaster
{
    public class UpdateSlabMasterRequest
    {
        public int SlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UpdateSlabMasterRequestValidator : AbstractValidator<UpdateSlabMasterRequest>
    {
        public UpdateSlabMasterRequestValidator()
        {
            RuleFor(x => x.SlabMasterID).GreaterThan(0);
            RuleFor(x => x.SlabName).NotEmpty().MaximumLength(80);
        }
    }
}
