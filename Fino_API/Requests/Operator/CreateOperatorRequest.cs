using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.Operator
{
    public class CreateOperatorRequest
    {
        public int ServiceID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public decimal MinimumRechargeAmount { get; set; }
        public decimal MaximumRechargeAmount { get; set; }
        public int RepeatRechargeInMinute { get; set; }
        public bool? IsTimelyBlocked { get; set; }
        public DateTime BlockedTill { get; set; }
        public bool? IsRandom { get; set; }
        public string OperatorImagePath { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Opcode { get; set; } = string.Empty;
        public string BusinessModel { get; set; } = string.Empty;
        public int NumberLength { get; set; }
        public string HSNCode { get; set; } = string.Empty;
        public string Prefix { get; set; } = string.Empty;
        public string AccountStartWith { get; set; } = string.Empty;
        public string SequenceLogic { get; set; } = string.Empty;
        public bool? IsBBPS { get; set; } = false;
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public string TollFreeNumber { get; set; } = string.Empty;
        public bool IsBillFetch { get; set; } = false;
        public bool IsPartialBill { get; set; } = false;
        public decimal UISequence { get; set; }
        public string BlockedAmount { get; set; } = string.Empty;
        public string AllowedFor { get; set; } = string.Empty;
        public bool IsViewRequired { get; set; } = false;
    }

    public class CreateOperatorRequestValidator : AbstractValidator<CreateOperatorRequest>
    {
        public CreateOperatorRequestValidator()
        {
            // ServiceID
            RuleFor(x => x.ServiceID)
                .GreaterThan(0).WithMessage(OperatorMessages.InvalidValue);

            // OperatorName
            RuleFor(x => x.OperatorName)
                .NotEmpty().WithMessage(OperatorMessages.RequiredField)
                .MaximumLength(100).WithMessage(OperatorMessages.InvalidLength);

            // MinimumRechargeAmount
            RuleFor(x => x.MinimumRechargeAmount)
                .GreaterThan(0).WithMessage(OperatorMessages.InvalidValue);

            // MaximumRechargeAmount
            RuleFor(x => x.MaximumRechargeAmount)
                .GreaterThan(0).WithMessage(OperatorMessages.InvalidValue)
                .GreaterThan(x => x.MinimumRechargeAmount)
                .WithMessage(OperatorMessages.InvalidValue);

            // RepeatRechargeInMinute
            RuleFor(x => x.RepeatRechargeInMinute)
                .GreaterThan(0).WithMessage(OperatorMessages.InvalidValue);

            // BlockedTill
            RuleFor(x => x.BlockedTill)
                .GreaterThan(DateTime.Now).WithMessage(OperatorMessages.InvalidDateRange);

            // OperatorImagePath
            RuleFor(x => x.OperatorImagePath)
                .NotEmpty().WithMessage(OperatorMessages.RequiredField)
                .MaximumLength(200).WithMessage(OperatorMessages.InvalidLength);

            // Opcode
            RuleFor(x => x.Opcode)
                .NotEmpty().WithMessage(OperatorMessages.RequiredField)
                .Length(4).WithMessage(OperatorMessages.InvalidLength);

            // BusinessModel
            RuleFor(x => x.BusinessModel)
                .NotEmpty().WithMessage(OperatorMessages.RequiredField)
                .MaximumLength(50).WithMessage(OperatorMessages.InvalidLength);

            // NumberLength
            RuleFor(x => x.NumberLength)
                .GreaterThan(0).WithMessage(OperatorMessages.InvalidValue);

            // HSNCode
            RuleFor(x => x.HSNCode)
                .NotEmpty().WithMessage(OperatorMessages.RequiredField)
                .Length(8).WithMessage(OperatorMessages.InvalidLength);

            // Prefix
            RuleFor(x => x.Prefix)
                .NotEmpty().WithMessage(OperatorMessages.RequiredField)
                .MaximumLength(10).WithMessage(OperatorMessages.InvalidLength);

            // AccountStartWith
            RuleFor(x => x.AccountStartWith)
                .NotEmpty().WithMessage(OperatorMessages.RequiredField)
                .MaximumLength(10).WithMessage(OperatorMessages.InvalidLength);

            // SequenceLogic
            RuleFor(x => x.SequenceLogic)
                .NotEmpty().WithMessage(OperatorMessages.RequiredField)
                .MaximumLength(50).WithMessage(OperatorMessages.InvalidLength);

            // IsBBPS
            RuleFor(x => x.IsBBPS)
                .NotNull().WithMessage(OperatorMessages.RequiredField);

            // MinLength
            RuleFor(x => x.MinLength)
                .GreaterThan(0).WithMessage(OperatorMessages.InvalidValue);

            // MaxLength
            RuleFor(x => x.MaxLength)
                .GreaterThan(0).WithMessage(OperatorMessages.InvalidValue)
                .GreaterThan(x => x.MinLength).WithMessage(OperatorMessages.InvalidValue);

            // AccountName
            RuleFor(x => x.AccountName)
                .NotEmpty().WithMessage(OperatorMessages.RequiredField)
                .MaximumLength(100).WithMessage(OperatorMessages.InvalidLength);

            // Remark
            RuleFor(x => x.Remark)
                .MaximumLength(500).WithMessage(OperatorMessages.InvalidLength);

            // TollFreeNumber
            RuleFor(x => x.TollFreeNumber)
                .Matches(@"^\d{10}$").WithMessage(OperatorMessages.InvalidFormat);

            // IsBillFetch
            RuleFor(x => x.IsBillFetch)
                .NotNull().WithMessage(OperatorMessages.RequiredField);

            // IsPartialBill
            RuleFor(x => x.IsPartialBill)
                .NotNull().WithMessage(OperatorMessages.RequiredField);

            // UISequence
            RuleFor(x => x.UISequence)
                .GreaterThan(0).WithMessage(OperatorMessages.InvalidValue);

            // BlockedAmount
            RuleFor(x => x.BlockedAmount)
                .MaximumLength(50).WithMessage(OperatorMessages.InvalidLength);

            // AllowedFor
            RuleFor(x => x.AllowedFor)
                .MaximumLength(50).WithMessage(OperatorMessages.InvalidLength);

            // IsViewRequired
            RuleFor(x => x.IsViewRequired)
                .NotNull().WithMessage(OperatorMessages.RequiredField);
        }
    }
}