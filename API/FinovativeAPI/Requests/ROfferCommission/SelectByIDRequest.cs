using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ROfferCommission
{
    public class SelectByIDRequest
    {
        private int _ROfferCommissionID;

        public int ROfferCommissionID
        {
            get => _ROfferCommissionID;
            set => _ROfferCommissionID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"ROfferCommission deleted with ROfferCommissionID:{ROfferCommissionID}";
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.ROfferCommissionID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabDetailMessage.InValidID);
        }
    }
}