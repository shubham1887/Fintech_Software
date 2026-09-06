using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.IPMaster
{
    public class GetIPMasterRequest
    {
        public int IpMasterID { get; set; }
        public int UserID { get; set; }
        public string IP { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetIPMasterRequestValidator : AbstractValidator<GetIPMasterRequest>
    {
        public GetIPMasterRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage(IPMasterMessages.InvalidValue);

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage(IPMasterMessages.InvalidValue);

            RuleFor(x => x.IP)
                .Matches(@"^(\d{1,3}\.){3}\d{1,3}$").When(x => !string.IsNullOrEmpty(x.IP))
                .WithMessage(IPMasterMessages.InvalidIPFormat);
        }
    }
}