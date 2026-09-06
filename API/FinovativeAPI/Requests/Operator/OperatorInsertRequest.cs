using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Operator
{
    public class OperatorInsertRequest
    {
        public CustomFile Document = new CustomFile();

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private decimal _MinimumRechargeAmount;

        public decimal MinimumRechargeAmount
        {
            get => _MinimumRechargeAmount;
            set => _MinimumRechargeAmount = value;
        }

        private decimal _MaximumRechargeAmount;

        public decimal MaximumRechargeAmount
        {
            get => _MaximumRechargeAmount;
            set => _MaximumRechargeAmount = value;
        }

        private int _RepeatRechargeInMinute;

        public int RepeatRechargeInMinute
        {
            get => _RepeatRechargeInMinute;
            set => _RepeatRechargeInMinute = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        public string Code
        {
            get;
            internal set;
        }

        private string _BusinessModel;

        public string BusinessModel
        {
            get => _BusinessModel.TrimIfNotNull();
            set => _BusinessModel = value.TrimIfNotNull();
        }

        private string _Opcode;

        public string Opcode
        {
            get => _Opcode.TrimIfNotNull();
            set => _Opcode = value.TrimIfNotNull();
        }

        private string _HSNCode;

        public string HSNCode
        {
            get => _HSNCode.TrimIfNotNull();
            set => _HSNCode = value.TrimIfNotNull();
        }

        private string _SequenceLogic;

        public string SequenceLogic
        {
            get => _SequenceLogic.TrimIfNotNull();
            set => _SequenceLogic = value.TrimIfNotNull();
        }

        private string _Prefix;

        public string Prefix
        {
            get => _Prefix.TrimIfNotNull();
            set => _Prefix = value.TrimIfNotNull();
        }

        private string _OperatorImage;

        public string OperatorImage
        {
            get => _OperatorImage;
            set => _OperatorImage = value;
        }

        private bool _IsBBPS;

        public bool IsBBPS
        {
            get => _IsBBPS;
            set => _IsBBPS = value;
        }

        private bool _IsBillFetch;

        public bool IsBillFetch
        {
            get { return _IsBillFetch; }
            set { _IsBillFetch = value; }
        }

        private bool _IsPartialBill;

        public bool IsPartialBill
        {
            get { return _IsPartialBill; }
            set { _IsPartialBill = value; }
        }

        private int _MaxLength;

        public int MaxLength
        {
            get { return _MaxLength; }
            set { _MaxLength = value; }
        }

        private int _MinLength;

        public int MinLength
        {
            get { return _MinLength; }
            set { _MinLength = value; }
        }

        private string _TollFreeNumber;

        public string TollFreeNumber
        {
            get { return _TollFreeNumber.TrimIfNotNull(); }
            set { _TollFreeNumber = value.TrimIfNotNull(); }
        }

        private string _AccountName;

        public string AccountName
        {
            get { return _AccountName.TrimIfNotNull(); }
            set { _AccountName = value.TrimIfNotNull(); }
        }

        private string _AccountStartWith;

        public string AccountStartWith
        {
            get { return _AccountStartWith.TrimIfNotNull(); }
            set { _AccountStartWith = value.TrimIfNotNull(); }
        }

        private string _Remark;

        public string Remark
        {
            get { return _Remark.TrimIfNotNull(); }
            set { _Remark = value.TrimIfNotNull(); }
        }

        private decimal _UISequence;

        public decimal UISequence
        {
            get => _UISequence;
            set => _UISequence = value;
        }

        private string _AllowedFor;

        public string AllowedFor
        {
            get => _AllowedFor.TrimIfNotNull();
            set => _AllowedFor = value.TrimIfNotNull();
        }

        private string _BlockedAmount;

        public string BlockedAmount
        {
            get => _BlockedAmount.TrimIfNotNull();
            set => _BlockedAmount = value.TrimIfNotNull();
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
            var validator = new OperatorRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class OperatorRequestValidator : AbstractValidator<OperatorInsertRequest>
    {
        public OperatorRequestValidator()
        {
            RuleFor(x => x.OperatorName)
                .Must(new Validation().IsValidString)
                .WithMessage(OperatorMessage.InValidOperator);

            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.OperatorMessage.Invalid);

            RuleFor(x => x.MinimumRechargeAmount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(OperatorMessage.InvalidMinimumRechargeAmount);

            RuleFor(x => x.MaximumRechargeAmount)
                .Must(new Validation().IsValidDecimal)
                .WithMessage(OperatorMessage.InvalidMaximumRechargeAmount);
            RuleFor(x => x.Opcode)
                .Must(new Validation().IsValidString)
                .WithMessage(OperatorMessage.InValidOpcode);
            RuleFor(x => x.BusinessModel)
                .Must(new Validation().IsValidString)
                .WithMessage(OperatorMessage.InValidBusinessModel);
            RuleFor(x => x.AllowedFor)
               .Must(new Validation().IsValidString)
               .WithMessage(OperatorMessage.InValidAllowedFor);
        }
    }

    internal class FileValidator : AbstractValidator<OperatorInsertRequest>
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