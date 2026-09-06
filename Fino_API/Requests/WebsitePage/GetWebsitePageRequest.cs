using FluentValidation;
namespace FintechApp.Requests.WebsitePage
{
    public class GetWebsitePageRequest
    {
        public int WebsitePageMasterID { get; set; }
    }

    public class GetWebsitePageRequestValidator : AbstractValidator<GetWebsitePageRequest>
    {
        public GetWebsitePageRequestValidator()
        {
            RuleFor(x => x.WebsitePageMasterID).GreaterThan(0);
        }
    }
}
