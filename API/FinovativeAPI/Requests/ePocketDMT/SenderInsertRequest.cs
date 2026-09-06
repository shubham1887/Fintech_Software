using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ePocketDMT
{
    public class SenderInsertRequest
    {
        private string _Name;

        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }

        private string _Mobile;

        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
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

        private string _BeneName;

        public string BeneName
        {
            get => _BeneName.ToTitleCase();
            set => _BeneName = value.ToTitleCase();
        }

        internal string IsValidModalState()
        {
            var validator = new SenderInsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SenderInsertRequestValidator : AbstractValidator<SenderInsertRequest>
    {
        public SenderInsertRequestValidator()
        {
            RuleFor(x => x.Name)
                .Must(new Validation().IsValidString)
                .WithMessage(SenderMessage.InvalidName);

            RuleFor(x => x.Mobile)
               .Must(new Validation().IsValidString)
               .WithMessage(SenderMessage.InvalidMobile);

            RuleFor(x => x.AccountNo)
               .Must(new Validation().IsValidAccountNumber)
               .WithMessage(BeneficiaryMessage.InvalidAccountNo);

            RuleFor(x => x.IFSC)
               .Must(new Validation().IsValidIFSC)
               .WithMessage(BeneficiaryMessage.InvalidIFSC);

            RuleFor(x => x.BankName)
               .Must(new Validation().IsValidString)
               .WithMessage(BeneficiaryMessage.InvalidBankName);
        }
    }
}