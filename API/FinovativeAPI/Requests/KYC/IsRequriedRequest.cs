using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.KYC
{
    public class IsRequriedRequest
    {
        private int _KycDocumentMasterID;

        public int KycDocumentMasterID
        {
            get => _KycDocumentMasterID;
            set => _KycDocumentMasterID = value;
        }

        private bool _IsRequired;

        public bool IsRequired
        {
            get => _IsRequired;
            set => _IsRequired = value;
        }

        internal string IsValidModalState()
        {
            var validator = new IsRequriedRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class IsRequriedRequestValidator : AbstractValidator<IsRequriedRequest>
    {
        public IsRequriedRequestValidator()
        {
            RuleFor(x => x.KycDocumentMasterID)
              .Must(new Validation().IsValidInt)
              .WithMessage(KYCMessage.InvalidDocument);
        }
    }
}