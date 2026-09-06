using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.DematGrowthLoss
{
    public class InsertRequest
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
        private int _ServiceID;  
        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }
        
        private int _ServiceSubscriptionPlanMasterID;
        public int ServiceSubscriptionPlanMasterID
        {
            get => _ServiceSubscriptionPlanMasterID;
            set => _ServiceSubscriptionPlanMasterID = value; 
        }

        private int _DOCInt;
        public int DOCInt
        {
            get => _DOCInt;
            set => _DOCInt = value;
        }
        private string _ServiceStatus;
        public string ServiceStatus
        {
            get => _ServiceStatus;
            set => _ServiceStatus = value;
        }
        private string _DOC;
        public string DOC
        {
            get => _DOC;
            set => _DOC = value;
        }
        private DateTime _ManualDate;

        public DateTime ManualDate
        {
            get => _ManualDate;
            set => _ManualDate = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
        internal class InsertRequestValidator : AbstractValidator<InsertRequest>
        {
            public InsertRequestValidator()
            {
                RuleFor(x => x.ServiceID)
                    .Must(new Validation().IsValidInt)
                    .WithMessage(DematGrowthLossMasterMessage.InvalidID);
            }
        }
    }
}
