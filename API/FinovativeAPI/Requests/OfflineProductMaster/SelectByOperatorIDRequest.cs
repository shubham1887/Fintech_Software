using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OfflineProductMaster
{
    public class SelectByOperatorIDRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _OpType;

        public string OpType
        {
            get => _OpType.TrimIfNotNull();
            set => _OpType = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByOperatorIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SelectByOperatorIDRequestValidator : AbstractValidator<SelectByOperatorIDRequest>
    {
        public SelectByOperatorIDRequestValidator()
        {
            RuleFor(x => x.OperatorID)
              .Must(new Validation().IsValidpositive)
              .WithMessage(Message.OperatorMessage.InValidOperator);
        }
    }
}