using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OperatorStateMapping
{
    public class SelectRequest
    {
        private int _OperatorStateMappingID;

        public int OperatorStateMappingID { get => _OperatorStateMappingID; set => _OperatorStateMappingID = value; }
        private bool _IsActive;
        public bool IsActive { get => _IsActive; set => _IsActive = value; }
        private bool _IsDelete;
        public bool IsDelete { get => _IsDelete; set => _IsDelete = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.OperatorStateMappingID)
              .Must(new Validation().IsValidInt)
              .WithMessage(OperatorStateMappingMessage.InValidID);
        }
    }
}