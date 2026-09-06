using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OperatorUserwise
{
    public class SelectActiveRequest
    {
        private int _OperatorUserwiseID;

        public int OperatorUserwiseID
        {
            get => _OperatorUserwiseID;
            set => _OperatorUserwiseID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _UsageLimit;

        public int UsageLimit
        {
            get => _UsageLimit;
            set => _UsageLimit = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectActiveRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Userwise operator ActivateDeactivate with UserID:{OperatorUserwiseID} IsActive:{IsActive}";
        }
    }

    internal class SelectActiveRequestValidator : AbstractValidator<SelectActiveRequest>
    {
        public SelectActiveRequestValidator()
        {
            RuleFor(x => x.OperatorID)
           .Must(new Validation().IsValidInt)
           .WithMessage(OperatorMessage.InValidOperator);
        }
    }
}