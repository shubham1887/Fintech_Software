using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SlabDetail
{
    public class CommissionUpdateRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _Adjustment;

        public string Adjustment
        {
            get => _Adjustment;
            set => _Adjustment = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        internal string IsValidModalState()
        {
            var validator = new CommissionUpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return (!success) ? failures[0].ToString() : StatusType.Ok;
        }
    }

    internal class CommissionUpdateRequestValidator : AbstractValidator<CommissionUpdateRequest>
    {
        public CommissionUpdateRequestValidator()
        {
            RuleFor(x => x.ServiceID)
             .Must(new Validation().IsValidInt)
             .WithMessage(OperatorMessage.InvalidServiceID);
            RuleFor(x => x.OperatorID)
            .Must(new Validation().IsValidInt)
            .WithMessage(OperatorMessage.InvalidOperator);

            RuleFor(x => x.Commission)
           .Must(new Validation().IsValidDecimal)
           .WithMessage(SlabDetailMessage.InvalidAmout);
        }
    }
}