using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.ProviderMaster
{
    public class UpdateProviderMasterRequest
    {
        public int ProviderID { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
    }

    public class UpdateProviderMasterRequestValidator : AbstractValidator<UpdateProviderMasterRequest>
    {
        public UpdateProviderMasterRequestValidator()
        {
            RuleFor(x => x.ProviderID)
                .GreaterThan(0).WithMessage(ProviderMasterMessages.InvalidValue);

            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage(ProviderMasterMessages.RequiredField);

            RuleFor(x => x.NewValue)
                .NotEmpty().WithMessage(ProviderMasterMessages.RequiredField);
        }
    }
}
