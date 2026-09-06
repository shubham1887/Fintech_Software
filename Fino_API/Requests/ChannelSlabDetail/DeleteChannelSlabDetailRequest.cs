using FluentValidation;

namespace FintechApp.Requests.ChannelSlabDetail
{
    public class DeleteChannelSlabDetailRequest
    {
        public int ChannelSlabDetailID { get; set; }
    }

    public class DeleteChannelSlabDetailRequestValidator : AbstractValidator<DeleteChannelSlabDetailRequest>
    {
        public DeleteChannelSlabDetailRequestValidator()
        {
            RuleFor(x => x.ChannelSlabDetailID).GreaterThan(0);
        }
    }
}
