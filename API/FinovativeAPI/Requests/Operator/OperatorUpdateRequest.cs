using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Operator
{
    public class OperatorUpdateRequest
    {
        private int _OperatorID;
        private int _ServiceID;
        private string _OperatorName;
        private decimal _MinimumRechargeAmount;
        private decimal _MaximumRechargeAmount;
        private int _RepeatRechargeInMinute;
        private string _BusinessModel;
        private string _Opcode;

        private string _HSNCode;
        private string _SequenceLogic;

        private string _AccountStartWith;
        private string _OperatorImage;

        public CustomFile Document = new CustomFile();

        public string AccountStartWith
        {
            get => _AccountStartWith.TrimIfNotNull();
            set => _AccountStartWith = value.TrimIfNotNull();
        }

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        public decimal MinimumRechargeAmount
        {
            get => _MinimumRechargeAmount;
            set => _MinimumRechargeAmount = value;
        }

        public decimal MaximumRechargeAmount
        {
            get => _MaximumRechargeAmount;
            set => _MaximumRechargeAmount = value;
        }

        public int RepeatRechargeInMinute
        {
            get => _RepeatRechargeInMinute;
            set => _RepeatRechargeInMinute = value;
        }

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        public string Code { get; internal set; }

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        public string BusinessModel
        {
            get => _BusinessModel.TrimIfNotNull();
            set => _BusinessModel = value.TrimIfNotNull();
        }

        public string Opcode
        {
            get => _Opcode.TrimIfNotNull();
            set => _Opcode = value.TrimIfNotNull();
        }

        public string HSNCode
        {
            get => _HSNCode.TrimIfNotNull();
            set => _HSNCode = value.TrimIfNotNull();
        }

        public string SequenceLogic
        {
            get => _SequenceLogic.TrimIfNotNull();
            set => _SequenceLogic = value.TrimIfNotNull();
        }

        public string OperatorImage
        {
            get => _OperatorImage.TrimIfNotNull();
            set => _OperatorImage = value.TrimIfNotNull();
        }

        private bool _IsViewRequired;

        public bool IsViewRequired
        {
            get => _IsViewRequired;
            set => _IsViewRequired = value;
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
            var validator = new UpdateFileValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        internal string IsValidModalState()
        {
            var validator = new OperatorUpdateValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class OperatorUpdateValidator : AbstractValidator<OperatorUpdateRequest>
    {
        public OperatorUpdateValidator()
        {
            RuleFor(x => x.OperatorID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.OperatorMessage.Invalid);

            RuleFor(x => x.OperatorName)
                .Must(new Validation().IsValidString)
                .WithMessage(OperatorMessage.InValidOperator);

            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.ServiceMessage.Invalid);

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

    internal class UpdateFileValidator : AbstractValidator<OperatorUpdateRequest>
    {
        public UpdateFileValidator()
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