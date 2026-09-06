using FluentValidation;

namespace FintechApp.Requests.UserChannelSlabMapping
{
    public class CreateUserChannelSlabMappingRequest
    {
        public int UserID { get; set; }
        public int ChannelSlabMasterID { get; set; }
        public int UplineUserID { get; set; }
    }

    public class CreateUserChannelSlabMappingRequestValidator : AbstractValidator<CreateUserChannelSlabMappingRequest>
    {
        public CreateUserChannelSlabMappingRequestValidator()
        {
            RuleFor(x => x.UserID).GreaterThan(0);
            RuleFor(x => x.ChannelSlabMasterID).GreaterThan(0);
        }
    }
}
