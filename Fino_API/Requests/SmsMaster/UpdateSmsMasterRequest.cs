using FluentValidation;

namespace FintechApp.Requests.SmsMaster
{
    public class UpdateSmsMasterRequest
    {
        public int SMSMasterID { get; set; }
        public string SMSString { get; set; } = string.Empty;
        public string BalanceURL { get; set; } = string.Empty;
        public string WhatsAppURL { get; set; } = string.Empty;
        public string APIName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string MethodType { get; set; } = string.Empty;
        public string HeaderJson { get; set; } = string.Empty;
        public string MethodTypeWhatsApp { get; set; } = string.Empty;
        public string HeaderJsonWhatsApp { get; set; } = string.Empty;
        public string MobileWithOrWithout91 { get; set; } = string.Empty;
        public string SMSOTPURL { get; set; } = string.Empty;
        public bool IsOTP { get; set; }
    }

    public class UpdateSmsMasterRequestValidator : AbstractValidator<UpdateSmsMasterRequest>
    {
        public UpdateSmsMasterRequestValidator()
        {
            RuleFor(x => x.SMSMasterID).GreaterThan(0);
            RuleFor(x => x.SMSString).NotEmpty();
            RuleFor(x => x.MethodType).NotEmpty();
        }
    }
}
