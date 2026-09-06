using FluentValidation;
namespace FintechApp.Requests.Inbox
{
    public class CreateInboxRequest { public int ServiceID { get; set; } public int OperatorID { get; set; } public int UserID { get; set; } public decimal Amount { get; set; } public string ConsumerNumber { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; public string Through { get; set; } = string.Empty; public string IPAddress { get; set; } = string.Empty; public string Param1 { get; set; } = string.Empty; }
    public class CreateInboxRequestValidator : AbstractValidator<CreateInboxRequest> { public CreateInboxRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
