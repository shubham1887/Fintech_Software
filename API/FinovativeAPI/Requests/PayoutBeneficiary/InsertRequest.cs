using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PayoutBeneficiary
{
    public class InsertRequest
    {
        public CustomFile Document = new CustomFile();
        public int UserID { get; set; }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _ActivePayoutName;

        public string ActivePayoutName
        {
            get => _ActivePayoutName.TrimIfNotNull();
            set => _ActivePayoutName = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
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

        private bool _IsNameVerified;

        public bool IsNameVerified
        {
            get => _IsNameVerified;
            set => _IsNameVerified = value;
        }
        internal string IsValidModalStateForDocument()
        {
            var validator = new FileValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
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
    internal class FileValidator : AbstractValidator<InsertRequest>
    {
        public FileValidator()
        {
            RuleFor(x => x.Document.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.Document.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);
        }
    }
}