using FluentValidation;

namespace FintechApp.Requests.ChannelSlabMaster
{
    public class UpdateChannelSlabMasterRequest
    {
        public int ChannelSlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int UplineUserID { get; set; }
    }

    public class UpdateChannelSlabMasterRequestValidator : AbstractValidator<UpdateChannelSlabMasterRequest>
    {
        public UpdateChannelSlabMasterRequestValidator()
        {
            RuleFor(x => x.ChannelSlabMasterID).GreaterThan(0);
            RuleFor(x => x.SlabName).NotEmpty().MaximumLength(80);
        }
    }
}
