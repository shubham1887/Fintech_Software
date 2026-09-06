using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class FetchDetailsForPincodeRequest
    {
        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new FetchDetailsForPincodeRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class FetchDetailsForPincodeRequestValidator : AbstractValidator<FetchDetailsForPincodeRequest>
    {
        public FetchDetailsForPincodeRequestValidator()
        {
            RuleFor(x => x.Pincode)
               .Must(new Validation().IsValidString)
               .WithMessage(Message.UserMessage.InvalidPinCode);
        }
    }
}