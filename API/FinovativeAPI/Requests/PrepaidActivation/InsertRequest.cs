using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PrepaidActivation
{
    public class InsertRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _ActivationType;

        public string ActivationType
        {
            get => _ActivationType.TrimIfNotNull();
            set => _ActivationType = value.TrimIfNotNull();
        }

        private string _AlternateNumber;

        public string AlternateNumber
        {
            get => _AlternateNumber.TrimIfNotNull();
            set => _AlternateNumber = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
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
            RuleFor(x => x.OperatorID)
                .Must(new Validation().IsValidInt)
                .WithMessage(OperatorMessage.InValidOperator);

            RuleFor(x => x.AlternateNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(TransactionMessage.InvalidConsumerNumber);

            RuleFor(x => x.ActivationType)
               .Must(new Validation().IsValidString)
               .WithMessage(PrepaidActivationMessage.InvalidType);

            RuleFor(x => x.Name)
             .Must(new Validation().IsValidString)
             .WithMessage(PrepaidActivationMessage.InvalidName);

            RuleFor(x => x.Address)
             .Must(new Validation().IsValidString)
             .WithMessage(PrepaidActivationMessage.InvalidAddress);

            RuleFor(x => x.Pincode)
             .Must(new Validation().IsValidString)
             .WithMessage(PrepaidActivationMessage.InvalidPincode);
        }
    }
}