using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AutoTransferUserwiseMapping
{
    public class UpdateRequest
    {
        private int _AutoTransferUserwiseMappingID;

        public int AutoTransferUserwiseMappingID
        {
            get => _AutoTransferUserwiseMappingID;
            set => _AutoTransferUserwiseMappingID = value;
        }

        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private decimal _MaxOutstanding;

        public decimal MaxOutstanding
        {
            get => _MaxOutstanding;
            set => _MaxOutstanding = value;
        }

        private decimal _RechargeLowBalanceReached;

        public decimal RechargeLowBalanceReached
        {
            get => _RechargeLowBalanceReached;
            set => _RechargeLowBalanceReached = value;
        }

        private decimal _RechargeAmountIncreasedBy;

        public decimal RechargeAmountIncreasedBy
        {
            get => _RechargeAmountIncreasedBy;
            set => _RechargeAmountIncreasedBy = value;
        }

        private decimal _DMTLowBalanceReached;

        public decimal DMTLowBalanceReached
        {
            get => _DMTLowBalanceReached;
            set => _DMTLowBalanceReached = value;
        }

        private decimal _DMTAmountIncreasedBy;

        public decimal DMTAmountIncreasedBy
        {
            get => _DMTAmountIncreasedBy;
            set => _DMTAmountIncreasedBy = value;
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
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

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.AutoTransferUserwiseMappingID)
                      .Must(new Validation().IsValidpositive)
                      .WithMessage(AutoTransferMessages.InValidUser);

            RuleFor(x => x.UserID)
             .Must(new Validation().IsValidpositive)
             .WithMessage(AutoTransferMessages.InValidUser);

            RuleFor(x => x.DMTAmountIncreasedBy)
                      .Must(new Validation().IsValidDecimal)
                      .WithMessage(AutoTransferMessages.InValidDMTAmountIncreasedBy);

            RuleFor(x => x.DMTLowBalanceReached)
                     .Must(new Validation().IsValidDecimal)
                     .WithMessage(AutoTransferMessages.InValidDMTLowBalanceReached);

            RuleFor(x => x.RechargeLowBalanceReached)
                     .Must(new Validation().IsValidDecimal)
                     .WithMessage(AutoTransferMessages.InValidRechargeLowBalanceReached);

            RuleFor(x => x.RechargeAmountIncreasedBy)
                     .Must(new Validation().IsValidDecimal)
                     .WithMessage(AutoTransferMessages.InValidRechargeAmountIncreasedBy);
        }
    }
}