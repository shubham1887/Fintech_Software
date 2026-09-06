using FluentValidation;
namespace FintechApp.Requests.BulkMessage
{
    public class UpdateBulkMessageRequest { public int BulkMessageID { get; set; } public int UserID { get; set; } public string Message { get; set; } = string.Empty; public bool IsSMS { get; set; } public bool IsEmail { get; set; } public bool IsWhatsUp { get; set; } public bool IsDelete { get; set; } }
    public class UpdateBulkMessageRequestValidator : AbstractValidator<UpdateBulkMessageRequest> { public UpdateBulkMessageRequestValidator() { RuleFor(x => x.BulkMessageID).GreaterThan(0); } }
}
