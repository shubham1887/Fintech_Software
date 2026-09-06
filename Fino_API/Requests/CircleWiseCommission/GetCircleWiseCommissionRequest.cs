using FluentValidation;

namespace FintechApp.Requests.CircleWiseCommission
{
    public class GetCircleWiseCommissionRequest { public int CircleWiseCommissionID { get; set; } public int SlabMasterID { get; set; } }
    public class GetCircleWiseCommissionRequestValidator : AbstractValidator<GetCircleWiseCommissionRequest>
    {
        public GetCircleWiseCommissionRequestValidator() { RuleFor(x => x.CircleWiseCommissionID).GreaterThan(0); }
    }
}
