using FluentValidation;

namespace FintechApp.Requests.DenominationMaster
{
    public class CreateDenominationMasterRequest
    {
        public string Description { get; set; } = string.Empty;
        public string DenominationRange { get; set; } = string.Empty;
        public bool IsStop { get; set; }
    }

    public class CreateDenominationMasterRequestValidator : AbstractValidator<CreateDenominationMasterRequest>
    {
        public CreateDenominationMasterRequestValidator()
        {
            RuleFor(x => x.DenominationRange).NotEmpty();
        }
    }
}
