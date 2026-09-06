using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserWisePlanLimitSetting
{
    public class UpdateRequest
    {
        private int _UserWisePlanLimitID;

        public int UserWisePlanLimitID
        {
            get => _UserWisePlanLimitID;
            set => _UserWisePlanLimitID = value;
        }

        private int _PlanHitCount;

        public int PlanHitCount
        {
            get => _PlanHitCount;
            set => _PlanHitCount = value;
        }
        
        private int _BillFetchHitCount;

        public int BillFetchHitCount
        {
            get => _BillFetchHitCount;
            set => _BillFetchHitCount = value;
        }

        private int _CircleHitCount;

        public int CircleHitCount
        {
            get => _CircleHitCount;
            set => _CircleHitCount = value;
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

    public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.UserWisePlanLimitID)
                  .Must(new Validation().IsValidInt)
                  .WithMessage(UserCreationMessage.InvalidID);

        }
    }
}