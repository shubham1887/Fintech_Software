using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PrepaidActivation
{
    public class UpdateRequest
    {
        private int _PrepaidActivationID;

        public int PrepaidActivationID
        {
            get => _PrepaidActivationID;
            set => _PrepaidActivationID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.PrepaidActivationID)
                .Must(new Validation().IsValidInt)
                .WithMessage(PrepaidActivationMessage.InvalidID);

            RuleFor(x => x.Status)
            .Must(new Validation().IsValidString)
            .WithMessage(PrepaidActivationMessage.InsertSuccess);
        }
    }
}