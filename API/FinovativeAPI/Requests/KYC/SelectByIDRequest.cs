using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.KYC
{
    public class SelectByIDRequest
    {
        private int _UserDocumentID;

        public int UserDocumentID
        {
            get => _UserDocumentID;
            set => _UserDocumentID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.UserDocumentID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(Message.KYCMessage.InvalidDocument);
        }
    }
}