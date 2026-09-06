using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Provider
{
    public class ProviderDetailsSelectRequest
    {
        private int _ProviderDetailID;
        private bool _IsActive;
        public int ProviderDetailID { get => _ProviderDetailID; set => _ProviderDetailID = value; }
        public bool IsActive { get => _IsActive; set => _IsActive = value; }

        internal string IsValidModalState()
        {
            var validator = new ProviderDetailsSelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Provider details deleted with :ProviderDetailID:{ProviderDetailID}";
        }
    }

    public class ProviderDetailsSelectRequestValidator : AbstractValidator<ProviderDetailsSelectRequest>
    {
        public ProviderDetailsSelectRequestValidator()
        {
            RuleFor(x => x.ProviderDetailID)
              .Must(new Validation().IsValidInt)
              .WithMessage(ProviderMessage.InValidProvider);
        }
    }
}