using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.ProviderMaster
{
    public class DeleteProviderMasterRequest
    {
        public int ProviderID { get; set; }
    }

    public class DeleteProviderMasterRequestValidator : AbstractValidator<DeleteProviderMasterRequest>
    {
        public DeleteProviderMasterRequestValidator()
        {
            RuleFor(x => x.ProviderID)
                .GreaterThan(0).WithMessage(ProviderMasterMessages.InvalidValue);
        }
    }
}
