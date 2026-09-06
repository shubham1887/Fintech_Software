using FluentValidation;

namespace FintechApp.Requests.CircleWiseCommission
{
    public class CreateCircleWiseCommissionRequest
    {
        public int SlabMasterID { get; set; }
        public int OperatorID { get; set; }
        public int StateID { get; set; }
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public decimal RetailerComm { get; set; }
        public string? RetailerCommType { get; set; }
        public string? RetailerCommissionCategory { get; set; }
        public decimal DistributorComm { get; set; }
        public string? DistributorCommType { get; set; }
        public string? DistributorCommissionCategory { get; set; }
        public decimal SuperDistributorComm { get; set; }
        public string? SuperDistributorCommType { get; set; }
        public string? SuperDistributorCommissionCategory { get; set; }
        public decimal ClientComm { get; set; }
        public string? ClientCommType { get; set; }
        public string? ClientCommissionCategory { get; set; }
    }
    public class CreateCircleWiseCommissionRequestValidator : AbstractValidator<CreateCircleWiseCommissionRequest>
    {
        public CreateCircleWiseCommissionRequestValidator() { RuleFor(x => x.SlabMasterID).GreaterThan(0); RuleFor(x => x.OperatorID).GreaterThan(0); }
    }
}
