using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.DematGrowthLoss
{
    public class UpdateRequest
    {

        private int _DematGrowthLossMasterID;

        public int DematGrowthLossMasterID
        {
            get => _DematGrowthLossMasterID;
            set => _DematGrowthLossMasterID = value;
        }
        private int _UserID;
        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
        private decimal _Amount;
        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _ServiceStatus;
        public string ServiceStatus
        {
            get => _ServiceStatus;
            set => _ServiceStatus = value;
        }

       
        private int _ServiceSubscriptionPlanMasterID;
        public int ServiceSubscriptionPlanMasterID
        {
            get => _ServiceSubscriptionPlanMasterID;
            set => _ServiceSubscriptionPlanMasterID = value;
        }
        private int _ServiceID;
        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }
        private DateTime _ManualDate;

        public DateTime ManualDate
        {
            get => _ManualDate;
            set => _ManualDate = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
        internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
        {
            public UpdateRequestValidator()
            {
                RuleFor(x => x.DematGrowthLossMasterID)
                    .Must(new Validation().IsValidInt)
                    .WithMessage(DematGrowthLossMasterMessage.InvalidID);

                RuleFor(x => x.Amount)
                  .Must(new Validation().IsValidDecimal)                 
                  .WithMessage(DematGrowthLossMasterMessage.InvalidAmount);

                RuleFor(x => x.ServiceStatus)
                   .Must(new Validation().IsValidString)
                   .WithMessage(DematGrowthLossMasterMessage.InvalidGrowthLoss);

                RuleFor(x => x._ServiceSubscriptionPlanMasterID)
                  .Must(new Validation().IsValidInt)
                  .WithMessage(DematGrowthLossMasterMessage.InvalidPlan);
            }
        }
    }

}

