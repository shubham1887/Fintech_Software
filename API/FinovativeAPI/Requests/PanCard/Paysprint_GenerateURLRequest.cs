using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PanCard
{
    public class Paysprint_GenerateURLRequest
    {
        private string _FirstName;

        public string FirstName
        {
            get => _FirstName.TrimIfNotNull();
            set => _FirstName = value.TrimIfNotNull();
        }

        private string _LastName;

        public string LastName
        {
            get => _LastName.TrimIfNotNull();
            set => _LastName = value.TrimIfNotNull();
        }

        private string _MiddleName;

        public string MiddleName
        {
            get => _MiddleName.TrimIfNotNull();
            set => _MiddleName = value.TrimIfNotNull();
        }

        private string _Mode;

        public string Mode
        {
            get => _Mode.TrimIfNotNull();
            set => _Mode = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _Gender;

        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }
        private string _PaysprintMerchantID;
        public string PaysprintMerchantID
        {
            get => _PaysprintMerchantID.TrimIfNotNull();
            set => _PaysprintMerchantID = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new Paysprint_GenerateURLRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Paysprint_GenerateURLRequestValidator : AbstractValidator<Paysprint_GenerateURLRequest>
    {
        public Paysprint_GenerateURLRequestValidator()
        {
            RuleFor(x => x.FirstName)
               .Must(new Validation().IsValidString)
               .WithMessage(PanMessage.InvalidFirmName);

            RuleFor(x => x.LastName)
                .Must(new Validation().IsValidString)
                .WithMessage(PanMessage.InvalidLastName);

            RuleFor(x => x.Mode)
               .Must(new Validation().IsValidString)
               .WithMessage(PanMessage.InvalidMode);
        }
    }
}