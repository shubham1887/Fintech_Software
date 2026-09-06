using FluentValidation;
namespace FintechApp.Requests.BulkMessage
{
    public class CreateBulkMessageRequest { public int UserID { get; set; } public string Message { get; set; } = string.Empty; public bool IsSMS { get; set; } public bool IsEmail { get; set; } public bool IsWhatsUp { get; set; } public bool IsDelete { get; set; } }
    public class CreateBulkMessageRequestValidator : AbstractValidator<CreateBulkMessageRequest> { public CreateBulkMessageRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
