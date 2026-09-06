using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Service
{
    public class InsertRequest
    {
        private string _ServiceName;
        private decimal _Charge;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull(); set => _ServiceName = value.TrimIfNotNull();
        }

        public decimal Charge { get => _Charge; set => _Charge = value; }

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
            RuleFor(x => x.ServiceName)
                .Must(new Validation().IsValidString)
                .WithMessage(ServiceMessage.InvalidName);

            RuleFor(x => x.Charge)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(ServiceMessage.InvalidCharge);
        }
    }
}