using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.Service
{
    public class CreateServiceRequest
    {
        public string Name { get; set; } = string.Empty;
        public string ServiceCategory { get; set; } = string.Empty;
        public string ServiceImagePath { get; set; } = string.Empty;
        public int UISequence { get; set; }
        public string HSNCode { get; set; } = string.Empty;
        public string HSNCodeForSelfInvoice { get; set; } = string.Empty;
        public decimal Charge { get; set; }
        public decimal MinBalance { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDisplay { get; set; } = true;
        public bool IsDefault { get; set; } = true;
        public bool IsOutside { get; set; } = true;
        public bool? IsBBPS { get; set; } = false;
        public bool IsSubscriptionAllow { get; set; } = true;
        public bool IsGSTInclusive { get; set; } = false;
        /// <summary>Report mapping (ServiceKinds constant). Empty = auto-detect from name/category.</summary>
        public string ReportKind { get; set; } = string.Empty;
    }

    public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
    {
        public CreateServiceRequestValidator()
        {
            // Name
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ServiceMessages.RequiredField)
                .MaximumLength(100).WithMessage(ServiceMessages.InvalidLength);

            // ServiceCategory
            RuleFor(x => x.ServiceCategory)
                .NotEmpty().WithMessage(ServiceMessages.RequiredField)
                .MaximumLength(50).WithMessage(ServiceMessages.InvalidLength);

            // HSNCode
            RuleFor(x => x.HSNCode)
                .NotEmpty().WithMessage(ServiceMessages.RequiredField)
                .Length(8).WithMessage(ServiceMessages.InvalidLength);

            // HSNCodeForSelfInvoice
            RuleFor(x => x.HSNCodeForSelfInvoice)
                .NotEmpty().WithMessage(ServiceMessages.RequiredField)
                .Length(8).WithMessage(ServiceMessages.InvalidLength);

            // Charge
            RuleFor(x => x.Charge)
                .GreaterThanOrEqualTo(0).WithMessage(ServiceMessages.InvalidValue);

            // MinBalance
            RuleFor(x => x.MinBalance)
                .GreaterThanOrEqualTo(0).WithMessage(ServiceMessages.InvalidValue);

            // IsBBPS
            RuleFor(x => x.IsBBPS)
                .NotNull().WithMessage(ServiceMessages.RequiredField);
        }
    }
}