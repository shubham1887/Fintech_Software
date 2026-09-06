using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Beneficiary
{
    public class CashFreeBeneInsertRequest
    {
        public int UserID { get; set; }

        private string _BeneficiaryID;

        public string BeneficiaryID
        {
            get => _BeneficiaryID.ToTitleCase();
            set => _BeneficiaryID = value.ToTitleCase();
        }

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

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
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

        private string _PinCode;

        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }

        private string _BeneMobileNumber;

        public string BeneMobileNumber
        {
            get => _BeneMobileNumber.TrimIfNotNull();
            set => _BeneMobileNumber = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private bool _IsNameVerified;

        public bool IsNameVerified
        {
            get => _IsNameVerified;
            set => _IsNameVerified = value;
        }

        internal string IsValidModalState()
        {
            var validator = new CashFreeBeneInsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class CashFreeBeneInsertRequestValidator : AbstractValidator<CashFreeBeneInsertRequest>
    {
        public CashFreeBeneInsertRequestValidator()
        {
            RuleFor(x => x.Name)
                .Must(new Validation().IsValidString)
                .WithMessage(BeneficiaryMessage.InvalidName);

            RuleFor(x => x.BeneMobileNumber)
              .Must(new Validation().IsValidString)
              .WithMessage(BeneficiaryMessage.InvalidBeneMobile);

            RuleFor(x => x.AccountNo)
              .Must(new Validation().IsValidAccountNumber)
              .WithMessage(BeneficiaryMessage.InvalidAccountNo);

            RuleFor(x => x.IFSC)
              .Must(new Validation().IsValidIFSC)
              .WithMessage(BeneficiaryMessage.InvalidIFSC);

            RuleFor(x => x.SenderID)
              .Must(new Validation().IsValidString)
              .WithMessage(BeneficiaryMessage.InvalidSender);

            RuleFor(x => x.SenderMobile)
             .Must(new Validation().IsValidString)
             .WithMessage(BeneficiaryMessage.InvalidMobile);

            RuleFor(x => x.PinCode)
            .Must(new Validation().IsValidString)
            .WithMessage(BeneficiaryMessage.InvalidPinCode);

            RuleFor(x => x.City)
            .Must(new Validation().IsValidString)
            .WithMessage(BeneficiaryMessage.InvalidCity);

            RuleFor(x => x.State)
            .Must(new Validation().IsValidString)
            .WithMessage(BeneficiaryMessage.InvalidState);
        }
    }
}