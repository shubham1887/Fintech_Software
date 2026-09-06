using FluentValidation;

namespace FintechApp.Requests.ChannelSlabMaster
{
    public class CreateChannelSlabMasterRequest
    {
        public string SlabName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int UplineUserID { get; set; }
    }

    public class CreateChannelSlabMasterRequestValidator : AbstractValidator<CreateChannelSlabMasterRequest>
    {
        public CreateChannelSlabMasterRequestValidator()
        {
            RuleFor(x => x.SlabName).NotEmpty().MaximumLength(80);
        }
    }
}
