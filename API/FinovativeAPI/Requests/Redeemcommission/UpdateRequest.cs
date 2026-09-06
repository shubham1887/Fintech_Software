using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Redeemcommission
{
    public class UpdateRequest
    {
        private int _RedeemcommissionID;

        public int RedeemcommissionID
        {
            get => _RedeemcommissionID;
            set => _RedeemcommissionID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _AdminRemark;

        public string AdminRemark
        {
            get => _AdminRemark.TrimIfNotNull();
            set => _AdminRemark = value.TrimIfNotNull();
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
            RuleFor(x => x.RedeemcommissionID)
            .Must(new Validation().IsValidInt)
            .WithMessage(Message.CommissionHoldMessage.InvalidID);
        }
    }
}