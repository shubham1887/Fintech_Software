using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.Requests.ROffer
{
    public class SelectOfferRequest
    {
        private string _ConsumerNumber;
        private int _OperatorID;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        public int OperatorID { get => _OperatorID; set => _OperatorID = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectOfferRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures.FirstOrDefault().ToString();
        }
    }

    internal class SelectOfferRequestValidator : AbstractValidator<SelectOfferRequest>
    {
        private readonly Validation validation = new Validation();

        public SelectOfferRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                .Must(validation.IsValidInt)
                .WithMessage(Message.OperatorMessage.InValidOperator);

            RuleFor(x => x.ConsumerNumber)
              .Must(validation.IsValidString)
              .WithMessage(Message.ROffer.InvalidConsumerNumber);
        }
    }
}