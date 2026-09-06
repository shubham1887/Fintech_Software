using FluentValidation;

namespace FintechApp.Requests.ProviderConfiguration
{
    public class CreateProviderConfigurationRequest
    {
        public int OperatorID { get; set; }
        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
        public string OpCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Lapu { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;
        public decimal AdminComm { get; set; }
        public string AdminCommType { get; set; } = "Fix";
        public bool IsActive { get; set; } = true;
        public string CommissionCategory { get; set; } = string.Empty;
        public int PendingCount { get; set; }
        public int PendingInMinute { get; set; }
        public int FailCount { get; set; }
        public int FailInMinute { get; set; }
    }

    public class CreateProviderConfigurationRequestValidator : AbstractValidator<CreateProviderConfigurationRequest>
    {
        public CreateProviderConfigurationRequestValidator()
        {
            RuleFor(x => x.OperatorID).GreaterThan(0);
            RuleFor(x => x.ProviderID).GreaterThan(0);
            RuleFor(x => x.ServiceID).GreaterThan(0);
            RuleFor(x => x.OpCode).NotEmpty().MaximumLength(20);
            RuleFor(x => x.AdminCommType).NotEmpty().MaximumLength(20);
        }
    }
}
