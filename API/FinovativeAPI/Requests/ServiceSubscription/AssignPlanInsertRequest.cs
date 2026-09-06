using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ServiceSubscription
{
    public class AssignPlanInsertRequest
    {
        private int _AssignPlanSubscriptionID;

        public int AssignPlanSubscriptionID
        {
            get => _AssignPlanSubscriptionID;
            set => _AssignPlanSubscriptionID = value;
        }

        private int _ServiceSubscriptionCreatePlanID;

        public int ServiceSubscriptionCreatePlanID
        {
            get => _ServiceSubscriptionCreatePlanID;
            set => _ServiceSubscriptionCreatePlanID = value;
        }

        private int _UserID;
        public int UserID 
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _PlanName;

        public string PlanName
        {
            get => _PlanName.TrimIfNotNull();
            set => _PlanName = value.TrimIfNotNull();
        }

        private decimal _PlanAmount;

        public decimal PlanAmount
        {
            get => _PlanAmount;
            set => _PlanAmount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new AssignPlanInsertRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class AssignPlanInsertRequestValidator : AbstractValidator<AssignPlanInsertRequest>
    {
        public AssignPlanInsertRequestValidator()
        {
            //RuleFor(x => x.PlanName)
            //    .Must(new Validation().IsValidString)
            //    .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidCharge);

            RuleFor(x => x.PlanAmount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidOriginalCharge);
        }
    }
}