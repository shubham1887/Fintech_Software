using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.EZYPayout
{
    public class BeneficiaryRequest
    {
        private string _BeneName;

        public string BeneName
        {
            get => _BeneName.ToTitleCase();
            set => _BeneName = value.ToTitleCase();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.ToTitleCase();
            set => _BankName = value.ToTitleCase();
        }

        private string _AccountNo;

        public string AccountNo
        {
            get => _AccountNo.TrimIfNotNull();
            set => _AccountNo = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private int _BankID;

        public int BankID
        {
            get => _BankID;
            set => _BankID = value;
        }

        private string _SenderMobile;

        public string SenderMobile
        {
            get => _SenderMobile.TrimIfNotNull();
            set => _SenderMobile = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new BeneficiaryRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class BeneficiaryRequestValidator : AbstractValidator<BeneficiaryRequest>
    {
        public BeneficiaryRequestValidator()
        {
            RuleFor(x => x.BeneName)
                .Must(new Validation().IsValidString)
                .WithMessage(BeneficiaryMessage.InvalidName);

            RuleFor(x => x.BankName)
               .Must(new Validation().IsValidString)
               .WithMessage(BeneficiaryMessage.InvalidBankName);

            RuleFor(x => x.AccountNo)
              .Must(new Validation().IsValidAccountNumber)
              .WithMessage(BeneficiaryMessage.InvalidAccountNo);

            RuleFor(x => x.IFSC)
              .Must(new Validation().IsValidIFSC)
              .WithMessage(BeneficiaryMessage.InvalidIFSC);
        }
    }
}