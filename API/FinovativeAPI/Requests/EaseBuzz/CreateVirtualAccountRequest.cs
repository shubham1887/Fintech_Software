using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.EaseBuzz
{
    public class CreateVirtualAccountRequest
    {         
        private string _AccountNumber;

        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new CreateVirtualAccountRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class CreateVirtualAccountRequestValidator : AbstractValidator<CreateVirtualAccountRequest>
    {
        public CreateVirtualAccountRequestValidator()
        {
            RuleFor(config => config.AccountNumber)
                .Must(new Validation().IsValidString)
                .WithMessage(EcollectionMessage.InvalidAccountNumber);

            RuleFor(config => config.IFSC)
               .Must(new Validation().IsValidString)
               .WithMessage(EcollectionMessage.InvalidIFSC);
        }
    }
}