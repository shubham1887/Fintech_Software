using FluentValidation;

namespace FintechApp.Requests.ChannelSlabDetail
{
    public class GetChannelSlabDetailRequest
    {
        public int ChannelSlabDetailID { get; set; }
        public int ChannelSlabMasterID { get; set; }
    }

    public class GetChannelSlabDetailRequestValidator : AbstractValidator<GetChannelSlabDetailRequest>
    {
        public GetChannelSlabDetailRequestValidator() { }
    }
}
