using FluentValidation;
namespace FintechApp.Requests.WebsitePage
{
    public class DeleteWebsitePageRequest
    {
        public int WebsitePageMasterID { get; set; }
    }

    public class DeleteWebsitePageRequestValidator : AbstractValidator<DeleteWebsitePageRequest>
    {
        public DeleteWebsitePageRequestValidator()
        {
            RuleFor(x => x.WebsitePageMasterID).GreaterThan(0);
        }
    }
}
