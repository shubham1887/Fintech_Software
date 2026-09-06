using FluentValidation;
using FluentValidation.Results;
using FintechApp.Messages;
using FintechApp.StaticModels;

namespace FintechApp.Requests.IPMaster
{
    public class DeleteIPMasterRequest
    {
        public int IpMasterID { get; set; }

        internal string IsValidModalState()
        {
            var validator = new DeleteIPMasterRequestValidator();
            ValidationResult results = validator.Validate(this);
            return results.IsValid ? StatusType.Ok : results.Errors[0].ToString();
        }
    }

    public class DeleteIPMasterRequestValidator : AbstractValidator<DeleteIPMasterRequest>
    {
        public DeleteIPMasterRequestValidator()
        {
            RuleFor(x => x.IpMasterID)
                .GreaterThan(0).WithMessage(IPMasterMessages.InvalidValue);
        }
    }
}