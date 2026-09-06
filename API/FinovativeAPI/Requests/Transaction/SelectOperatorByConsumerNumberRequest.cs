using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class SelectOperatorByConsumerNumberRequest
    {
        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SelectOperatorByConsumerNumberRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectOperatorByConsumerNumberRequestValidator : AbstractValidator<SelectOperatorByConsumerNumberRequest>
    {
        public SelectOperatorByConsumerNumberRequestValidator()
        {
            RuleFor(x => x.ConsumerNumber)
                .Must(new Validation().IsValidMobileNumber)
                .WithMessage(UserMessage.InvalidUser);
        }
    }
}