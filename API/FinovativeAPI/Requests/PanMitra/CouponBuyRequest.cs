using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
namespace Project.Requests.PanMitra
{
    public class CouponBuyRequest
    {
        private string _VleID;
        public string VleID
        {
            get => _VleID.TrimIfNotNull();
            set => _VleID = value.TrimIfNotNull();
        }
        private int _OperatorID;
        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }
        private string _Qty;
        public string Qty
        {
            get => _Qty.TrimIfNotNull();
            set => _Qty = value.TrimIfNotNull();
        }
        internal string IsValidModalState()
        {
            var validator = new CouponBuyRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class CouponBuyRequestValidator : AbstractValidator<CouponBuyRequest>
    {
        public CouponBuyRequestValidator()
        {
            RuleFor(x => x.VleID)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.PanMessage.InvalidVleID);

            RuleFor(x => x.OperatorID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.PanMessage.InvalidOperator);
            RuleFor(x => x.Qty)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.PanMessage.InvalidQty);

        }
    }
}

