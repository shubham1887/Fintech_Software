using FluentValidation;
namespace FintechApp.Requests.ServiceAndUserwiseSubscription
{
    public class UpdateServiceAndUserwiseSubscriptionRequest { public int ServiceAndUserwiseSubscriptionID { get; set; } public int UserTypeID { get; set; } public int UserID { get; set; } public int ServiceID { get; set; } public string SubscriptionType { get; set; } = string.Empty; public decimal OriginalCharge { get; set; } public decimal Charge { get; set; } public DateTime EndDate { get; set; } = DateTime.MinValue; public string Remark { get; set; } = string.Empty; public string LoginID { get; set; } = string.Empty; public bool IsAutoDebit { get; set; } public int ServiceSubscriptionPlanMasterID { get; set; } public string Status { get; set; } = string.Empty; public int DaysCount { get; set; } }
    public class UpdateServiceAndUserwiseSubscriptionRequestValidator : AbstractValidator<UpdateServiceAndUserwiseSubscriptionRequest> { public UpdateServiceAndUserwiseSubscriptionRequestValidator() { RuleFor(x => x.ServiceAndUserwiseSubscriptionID).GreaterThan(0); } }
}
