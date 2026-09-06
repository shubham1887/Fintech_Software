using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserBankDetails
{
    public class InsertRequest
    {
        public CustomFile Document = new CustomFile();

        private int _UserBankDetailsID;

        public int UserBankDetailsID
        {
            get => _UserBankDetailsID;
            set => _UserBankDetailsID = value;
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

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

        private string _HolderName;

        public string HolderName
        {
            get => _HolderName.TrimIfNotNull();
            set => _HolderName = value.TrimIfNotNull();
        }

        private string _Branch;

        public string Branch
        {
            get => _Branch.TrimIfNotNull();
            set => _Branch = value.TrimIfNotNull();
        }

        private string _UPINumber;

        public string UPINumber
        {
            get => _UPINumber.TrimIfNotNull();
            set => _UPINumber = value.TrimIfNotNull();
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
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.AccountNumber)
                .Must(new Validation().IsValidString)
                .WithMessage(UserBankDetailsMessage.InvalidAccount);
            RuleFor(x => x.BankName)
               .Must(new Validation().IsValidString)
               .WithMessage(UserBankDetailsMessage.InvalidBank);
            RuleFor(x => x.IFSC)
               .Must(new Validation().IsValidIFSC)
               .WithMessage(UserBankDetailsMessage.InvalidIFSC);
            RuleFor(x => x.HolderName)
              .Must(new Validation().IsValidString)
              .WithMessage(UserBankDetailsMessage.InvalidHolderName);
            RuleFor(x => x.Branch)
              .Must(new Validation().IsValidString)
              .WithMessage(UserBankDetailsMessage.InvalidBranch);
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