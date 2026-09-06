using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderResponse
{
    public class UpdateSampleResponseRequest
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _ProviderResponseMasterSampleID;

        public int ProviderResponseMasterSampleID
        {
            get => _ProviderResponseMasterSampleID;
            set => _ProviderResponseMasterSampleID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateSampleResponseRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateSampleResponseRequestValidator : AbstractValidator<UpdateSampleResponseRequest>
    {
        public UpdateSampleResponseRequestValidator()
        {
            RuleFor(x => x.ProviderID)
               .Must(new Validation().IsValidInt)
               .WithMessage(ProviderResponseMessage.InvalidProvider);

            RuleFor(x => x.ProviderResponseMasterSampleID)
               .Must(new Validation().IsValidInt)
               .WithMessage(ProviderResponseMessage.InvalidSampleResponse);
        }
    }
}