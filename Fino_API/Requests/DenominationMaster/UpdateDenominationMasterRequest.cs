using FluentValidation;

namespace FintechApp.Requests.DenominationMaster
{
    public class UpdateDenominationMasterRequest
    {
        public int DenominationMasterID { get; set; }
        public string Description { get; set; } = string.Empty;
        public string DenominationRange { get; set; } = string.Empty;
        public bool IsStop { get; set; }
    }

    public class UpdateDenominationMasterRequestValidator : AbstractValidator<UpdateDenominationMasterRequest>
    {
        public UpdateDenominationMasterRequestValidator()
        {
            RuleFor(x => x.DenominationMasterID).GreaterThan(0);
            RuleFor(x => x.DenominationRange).NotEmpty();
        }
    }
}
