using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderConfigurationAdminCommission
{
    public class SelecRequest
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelecRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelecRequestValidator : AbstractValidator<SelecRequest>
    {
        public SelecRequestValidator()
        {
            RuleFor(x => x.ProviderID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.ProviderMessage.InValidProvider);
            RuleFor(x => x.ServiceID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.ServiceMessage.InvalidServideID);
        }
    }
}