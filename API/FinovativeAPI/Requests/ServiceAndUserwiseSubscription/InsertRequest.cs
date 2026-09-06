using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.ServiceAndUserwiseSubscription
{
    public class InsertRequest
    {
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

        private DateTime _EndDate;

        public DateTime EndDate
        {
            get => _EndDate;
            set => _EndDate = value;
        }

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


        private bool _IsAutoDebit;

        public bool IsAutoDebit
        {
            get => _IsAutoDebit;
            set => _IsAutoDebit = value;
        }

        private decimal _Charge;

        public decimal Charge
        {
            get => _Charge;
            set => _Charge = value;
        }

        private decimal _OriginalCharge;

        public decimal OriginalCharge
        {
            get => _OriginalCharge;
            set => _OriginalCharge = value;
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private string _SubscriptionType;

        public string SubscriptionType
        {
            get => _SubscriptionType.TrimIfNotNull();
            set => _SubscriptionType = value.TrimIfNotNull();
        }

        private string _LoginID;

        public string LoginID
        {
            get => _LoginID.TrimIfNotNull();
            set => _LoginID = value.TrimIfNotNull();
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

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {           
            RuleFor(x => x.Charge)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidCharge);


            RuleFor(x => x.OriginalCharge)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidOriginalCharge);

            RuleFor(x => x)
                .Must(x => x.Charge <= x.OriginalCharge)
                .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidChargeAndOriginalCharge);


            RuleFor(x => x.SubscriptionType)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.ServiceAndUserwiseSubscriptionMessage.InvalidSubscriptionType);

        }
    }
}