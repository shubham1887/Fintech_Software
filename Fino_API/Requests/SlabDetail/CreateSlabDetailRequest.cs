using FluentValidation;

namespace FintechApp.Requests.SlabDetail
{
    public class CreateSlabDetailRequest
    {
        public int SlabMasterID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public decimal RetailerComm { get; set; }
        public string RetailerCommType { get; set; } = string.Empty;
        public string RetailerCommissionCategory { get; set; } = string.Empty;
        public decimal DistributorComm { get; set; }
        public string DistributorCommType { get; set; } = string.Empty;
        public string DistributorCommissionCategory { get; set; } = string.Empty;
        public decimal SuperDistributorComm { get; set; }
        public string SuperDistributorCommType { get; set; } = string.Empty;
        public string SuperDistributorCommissionCategory { get; set; } = string.Empty;
        public decimal ClientComm { get; set; }
        public string ClientCommType { get; set; } = string.Empty;
        public string ClientCommissionCategory { get; set; } = string.Empty;
        public decimal RealRetailerComm { get; set; }
        public string RealRetailerCommType { get; set; } = string.Empty;
        public string RealRetailerCommissionCategory { get; set; } = string.Empty;
        public decimal RealDistributorComm { get; set; }
        public string RealDistributorCommType { get; set; } = string.Empty;
        public string RealDistributorCommissionCategory { get; set; } = string.Empty;
        public decimal RealSuperDistributorComm { get; set; }
        public string RealSuperDistributorCommType { get; set; } = string.Empty;
        public string RealSuperDistributorCommissionCategory { get; set; } = string.Empty;
        public decimal RealClientComm { get; set; }
        public string RealClientCommType { get; set; } = string.Empty;
        public string RealClientCommissionCategory { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class CreateSlabDetailRequestValidator : AbstractValidator<CreateSlabDetailRequest>
    {
        public CreateSlabDetailRequestValidator()
        {
            RuleFor(x => x.SlabMasterID).GreaterThan(0);
            RuleFor(x => x.ServiceID).GreaterThan(0);
            RuleFor(x => x.OperatorID).GreaterThan(0);
            RuleFor(x => x.FromAmount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ToAmount).GreaterThan(0);
        }
    }
}
