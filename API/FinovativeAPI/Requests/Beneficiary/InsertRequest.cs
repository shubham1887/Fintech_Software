using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Beneficiary
{
    public class InsertRequest
    {
        private string _DMTName;

        [DefaultValue("")]
        public string DMTName
        {
            get => _DMTName.TrimIfNotNull();
            set => _DMTName = value.TrimIfNotNull();
        }

        public int UserID { get; set; }

        private string _Name;

        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
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

        private string _SenderID;

        public string SenderID
        {
            get => _SenderID.TrimIfNotNull();
            set => _SenderID = value.TrimIfNotNull();
        }

        private string _OTP;

        public string OTP
        {
            get => _OTP.TrimIfNotNull();
            set => _OTP = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
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

        private string _AccountType;

        public string AccountType
        {
            get => _AccountType.TrimIfNotNull();
            set => _AccountType = value.TrimIfNotNull();
        }

        private bool _IsNameVerified;

        public bool IsNameVerified
        {
            get => _IsNameVerified;
            set => _IsNameVerified = value;
        }

        private string _Pipe;

        public string Pipe
        {
            get => _Pipe.TrimIfNotNull();
            set => _Pipe = value.TrimIfNotNull();
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
            /*RuleFor(x => x.UserID)
            .Must(new Validation().IsValidInt)
            .WithMessage(BeneficiaryMessage.InvalidUserID);*/

            RuleFor(x => x.Name)
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