using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Demat
{
    public class InitRequest
    {
        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.TrimIfNotNull();
            set => _PersonName = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber.TrimIfNotNull();
            set => _PanNumber = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InitRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InitRequestValidator : AbstractValidator<InitRequest>
    {
        public InitRequestValidator()
        {
            RuleFor(x => x.MobileNumber)
                .Must(new Validation().IsValidString)
                .WithMessage(DematMessage.InvalidMobile);

            RuleFor(x => x.PersonName)
            .Must(new Validation().IsValidString)
            .WithMessage(DematMessage.InValidName);

            RuleFor(x => x.EmailID)
           .Must(new Validation().IsValidString)
           .WithMessage(DematMessage.InValidEmailID);

            RuleFor(x => x.PanNumber)
          .Must(new Validation().IsValidString)
          .WithMessage(DematMessage.InValidPanNumber);
        }
    }
}