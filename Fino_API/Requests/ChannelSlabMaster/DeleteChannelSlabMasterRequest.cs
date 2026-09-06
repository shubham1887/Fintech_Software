using FluentValidation;

namespace FintechApp.Requests.ChannelSlabMaster
{
    public class DeleteChannelSlabMasterRequest
    {
        public int ChannelSlabMasterID { get; set; }
    }

    public class DeleteChannelSlabMasterRequestValidator : AbstractValidator<DeleteChannelSlabMasterRequest>
    {
        public DeleteChannelSlabMasterRequestValidator()
        {
            RuleFor(x => x.ChannelSlabMasterID).GreaterThan(0);
        }
    }
}
