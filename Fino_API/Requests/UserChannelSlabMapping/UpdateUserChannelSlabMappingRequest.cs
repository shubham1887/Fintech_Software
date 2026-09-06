using FluentValidation;

namespace FintechApp.Requests.UserChannelSlabMapping
{
    public class UpdateUserChannelSlabMappingRequest
    {
        public int UserChannelSlabMappingID { get; set; }
        public int UserID { get; set; }
        public int ChannelSlabMasterID { get; set; }
        public int UplineUserID { get; set; }
    }

    public class UpdateUserChannelSlabMappingRequestValidator : AbstractValidator<UpdateUserChannelSlabMappingRequest>
    {
        public UpdateUserChannelSlabMappingRequestValidator()
        {
            RuleFor(x => x.UserChannelSlabMappingID).GreaterThan(0);
            RuleFor(x => x.UserID).GreaterThan(0);
            RuleFor(x => x.ChannelSlabMasterID).GreaterThan(0);
        }
    }
}
