using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OperatorUserwise
{
    public class InsertRequest
    {
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

        private int _OperatorUserwiseID;

        public int OperatorUserwiseID
        {
            get => _OperatorUserwiseID;
            set => _OperatorUserwiseID = value;
        }

        private string _BlockedAmount;

        public string BlockedAmount
        {
            get => _BlockedAmount.TrimIfNotNull();
            set => _BlockedAmount = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Userwise operator update with UserID:{UserID},OperatorID:{OperatorID},UsageLimit:{UsageLimit}";
        }
    }

    public class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.UserID)
                  .Must(new Validation().IsValidInt)
                  .WithMessage(Message.UserMessage.InvalidUserID);
            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.OperatorMessage.InvalidServiceID);
            RuleFor(x => x.OperatorID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.OperatorMessage.InvalidOperatorID);
            RuleFor(x => x.UsageLimit).GreaterThan(-1)
             .WithMessage(Message.OperatorMessage.UsageLimit);
        }
    }
}