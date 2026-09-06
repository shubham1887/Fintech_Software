using FluentValidation;
namespace FintechApp.Requests.RichTextMedia
{
    public class UpdateRichTextMediaRequest { public int ID { get; set; } public string Key { get; set; } = string.Empty; public string Content { get; set; } = string.Empty; }
    public class UpdateRichTextMediaRequestValidator : AbstractValidator<UpdateRichTextMediaRequest> { public UpdateRichTextMediaRequestValidator() { RuleFor(x => x.ID).GreaterThan(0); } }
}
