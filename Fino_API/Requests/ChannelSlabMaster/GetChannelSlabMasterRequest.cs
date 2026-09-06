using FluentValidation;

namespace FintechApp.Requests.ChannelSlabMaster
{
    public class GetChannelSlabMasterRequest
    {
        public int ChannelSlabMasterID { get; set; }
        public int UplineUserID { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetChannelSlabMasterRequestValidator : AbstractValidator<GetChannelSlabMasterRequest>
    {
        public GetChannelSlabMasterRequestValidator() { }
    }
}
