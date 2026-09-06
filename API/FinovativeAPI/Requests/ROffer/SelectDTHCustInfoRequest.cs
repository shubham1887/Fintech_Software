using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.Requests.ROffer
{
    public class SelectDTHCustInfoRequest
    {
        private int _OperatorID;
        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        public int OperatorID { get => _OperatorID; set => _OperatorID = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectDTHCustInfoRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures.FirstOrDefault().ToString();
        }
    }

    internal class SelectDTHCustInfoRequestValidator : AbstractValidator<SelectDTHCustInfoRequest>
    {
        private readonly Validation validation = new Validation();

        public SelectDTHCustInfoRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                .Must(validation.IsValidInt)
                .WithMessage(Project.Message.OperatorMessage.InValidOperator);
        }
    }
}