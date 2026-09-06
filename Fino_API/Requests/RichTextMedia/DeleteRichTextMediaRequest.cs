using FluentValidation;
namespace FintechApp.Requests.RichTextMedia
{
    public class DeleteRichTextMediaRequest { public int ID { get; set; } }
    public class DeleteRichTextMediaRequestValidator : AbstractValidator<DeleteRichTextMediaRequest> { public DeleteRichTextMediaRequestValidator() { RuleFor(x => x.ID).GreaterThan(0); } }
}
