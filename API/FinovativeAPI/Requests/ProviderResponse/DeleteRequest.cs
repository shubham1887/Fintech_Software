using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderResponse
{
    public class DeleteRequest
    {
        private int _ProviderResponseMasterID;

        public int ProviderResponseMasterID { get => _ProviderResponseMasterID; set => _ProviderResponseMasterID = value; }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.ProviderResponseMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ProviderResponseMessage.InvalidResponse);
        }
    }
}