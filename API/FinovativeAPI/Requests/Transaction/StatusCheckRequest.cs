using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class StatusCheckRequest
    {
        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        private int _DateInteger;

        public int DateInteger
        {
            get => _DateInteger;
            set => _DateInteger = value;
        }

        internal string IsValidModalState()
        {
            var validator = new StatusCheckRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class StatusCheckRequestValidator : AbstractValidator<StatusCheckRequest>
    {
        public StatusCheckRequestValidator()
        {
            RuleFor(x => x.APIUserRequestID)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.TransactionMessage.InvalidRequestID);

            //RuleFor(x => x.DateInteger)
            //    .Must(new Validation().IsValidInt)
            //    .WithMessage(Message.TransactionMessage.InvalidDate);
        }
    }
}