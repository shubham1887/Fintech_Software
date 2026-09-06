using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserWisePlanLimitSetting
{
    public class InsertRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
        
        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private int _CircleHitCount;

        public int CircleHitCount
        {
            get => _CircleHitCount;
            set => _CircleHitCount = value;
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
        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserWisePlanLimitMessage.InvalidUserID);
           
        }
    }
}