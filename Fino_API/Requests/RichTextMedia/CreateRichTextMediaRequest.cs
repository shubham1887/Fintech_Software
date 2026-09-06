using FluentValidation;
namespace FintechApp.Requests.RichTextMedia
{
    public class CreateRichTextMediaRequest { public string Key { get; set; } = string.Empty; public string Content { get; set; } = string.Empty; }
    public class CreateRichTextMediaRequestValidator : AbstractValidator<CreateRichTextMediaRequest> { public CreateRichTextMediaRequestValidator() {  } }
}
