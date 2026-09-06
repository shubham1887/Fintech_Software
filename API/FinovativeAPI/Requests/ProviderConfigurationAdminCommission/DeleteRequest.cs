using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderConfigurationAdminCommission
{
    public class DeleteRequest
    {
        private int _ProviderConfigurationAdminCommissionID;

        public int ProviderConfigurationAdminCommissionID
        {
            get => _ProviderConfigurationAdminCommissionID;
            set => _ProviderConfigurationAdminCommissionID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DeleteRequestRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestRequestValidator()
        {
            RuleFor(x => x.ProviderConfigurationAdminCommissionID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabMessage.InvalidID);
        }
    }
}