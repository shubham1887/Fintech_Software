using FluentValidation;

namespace FintechApp.Requests.ChannelSlabDetail
{
    public class CreateChannelSlabDetailRequest
    {
        public int ChannelSlabMasterID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public decimal Commission { get; set; }
        public string CommissionType { get; set; } = "Fix";
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public int UplineUserID { get; set; }
        public decimal RealCommission { get; set; }
        public string RealCommissionType { get; set; } = "Fix";
        public string GeneralCommissionCategory { get; set; } = string.Empty;
        public string RealCommissionCategory { get; set; } = string.Empty;
        public decimal UplineCommission { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateChannelSlabDetailRequestValidator : AbstractValidator<CreateChannelSlabDetailRequest>
    {
        public CreateChannelSlabDetailRequestValidator()
        {
            RuleFor(x => x.ChannelSlabMasterID).GreaterThan(0);
            RuleFor(x => x.ServiceID).GreaterThan(0);
            RuleFor(x => x.OperatorID).GreaterThan(0);
            RuleFor(x => x.FromAmount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ToAmount).GreaterThan(0);
        }
    }
}
