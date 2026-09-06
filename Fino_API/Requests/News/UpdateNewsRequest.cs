using FluentValidation;

namespace FintechApp.Requests.News
{
    public class UpdateNewsRequest
    {
        public int NewsID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsView { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string HyperLink { get; set; } = string.Empty;
    }

    public class UpdateNewsRequestValidator : AbstractValidator<UpdateNewsRequest>
    {
        public UpdateNewsRequestValidator()
        {
            RuleFor(x => x.NewsID).GreaterThan(0);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        }
    }
}
