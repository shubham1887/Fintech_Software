using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Service
{
    public class UpdateRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private decimal _Charge;

        public decimal Charge
        {
            get => _Charge;
            set => _Charge = value;
        }

        private decimal _MinBalance;

        public decimal MinBalance
        {
            get => _MinBalance;
            set => _MinBalance = value;
        }

        private string _ServiceCategory;

        public string ServiceCategory
        {
            get => _ServiceCategory.TrimIfNotNull();
            set => _ServiceCategory = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ServiceMessage.Invalid);

            RuleFor(x => x.ServiceName)
                .Must(new Validation().IsValidString)
                .WithMessage(ServiceMessage.InvalidName);

            RuleFor(x => x.Charge)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(ServiceMessage.InvalidCharge);

            RuleFor(x => x.MinBalance)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(ServiceMessage.InvalidMinBalance);

            RuleFor(x => x.ServiceCategory)
                .Must(new Validation().IsValidString)
                .WithMessage(ServiceMessage.InvalidName);
        }
    }
}