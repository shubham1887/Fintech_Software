using FluentValidation;
namespace FintechApp.Requests.RichTextMedia
{
    public class GetRichTextMediaRequest { public int ID { get; set; } }
    public class GetRichTextMediaRequestValidator : AbstractValidator<GetRichTextMediaRequest> { public GetRichTextMediaRequestValidator() { RuleFor(x => x.ID).GreaterThan(0); } }
}
