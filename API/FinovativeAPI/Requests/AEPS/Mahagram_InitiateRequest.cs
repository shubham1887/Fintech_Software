using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AEPS
{
    public class Mahagram_InitiateRequest
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _RefernceNumber;

        public string RefernceNumber
        {
            get => _RefernceNumber.TrimIfNotNull();
            set => _RefernceNumber = value.TrimIfNotNull();
        }

        private string _AadharNumber;

        public string AadharNumber
        {
            get => _AadharNumber.TrimIfNotNull();
            set => _AadharNumber = value.TrimIfNotNull();
        }

        private string _BankIIN;

        public string BankIIN
        {
            get => _BankIIN.TrimIfNotNull();
            set => _BankIIN = value.TrimIfNotNull();
        }

        private string _Through;

        public string Through
        {
            get => _Through.TrimIfNotNull();
            set => _Through = value.TrimIfNotNull();
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new Mahagram_InitiateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Mahagram_InitiateRequestValidator : AbstractValidator<Mahagram_InitiateRequest>
    {
        public Mahagram_InitiateRequestValidator()
        {
            RuleFor(x => x.Amount)
            .Must(new Validation().IsValidDecimal)
            .WithMessage(AEPSMessage.InvalidAmount);

            RuleFor(x => x.Type)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidType);

            RuleFor(x => x.RefernceNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidRequestID);

            RuleFor(x => x.Through)
            .Must(new Validation().IsValidString)
            .WithMessage(AEPSMessage.InvalidRequestID);
        }
    }
}