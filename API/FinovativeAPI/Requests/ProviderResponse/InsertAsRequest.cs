using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Projec.Requests.ProviderResponse
{
    public class InsertAsRequest
    {
        private int _Provider;

        public int Provider { get => _Provider; set => _Provider = value; }

        private int _CopyFromProviderID;

        public int CopyFromProviderID { get => _CopyFromProviderID; set => _CopyFromProviderID = value; }

        internal string IsValidModalState()
        {
            var validator = new InsertAsRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertAsRequestValidator : AbstractValidator<InsertAsRequest>
    {
        public InsertAsRequestValidator()
        {
            RuleFor(x => x.Provider)
               .Must(new Validation().IsValidInt)
               .WithMessage(ProviderResponseMessage.InvalidProvider);

            RuleFor(x => x.CopyFromProviderID)
               .Must(new Validation().IsValidInt)
               .WithMessage(ProviderResponseMessage.InvalidProvider);
        }
    }
}