using FluentValidation;
namespace FintechApp.Requests.WebsitePage
{
    public class UpdateWebsitePageRequest
    {
        public int WebsitePageMasterID { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public string SectionsJson { get; set; } = string.Empty;
        public int MenuOrder { get; set; }
        public bool ShowInMenu { get; set; } = true;
        public bool IsPublished { get; set; }
    }

    public class UpdateWebsitePageRequestValidator : AbstractValidator<UpdateWebsitePageRequest>
    {
        public UpdateWebsitePageRequestValidator()
        {
            RuleFor(x => x.WebsitePageMasterID).GreaterThan(0);
            RuleFor(x => x.Slug).NotEmpty().MaximumLength(100).Matches("^[a-z0-9-]+$")
                .WithMessage("Slug must be lowercase letters, numbers and hyphens only.");
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        }
    }
}
