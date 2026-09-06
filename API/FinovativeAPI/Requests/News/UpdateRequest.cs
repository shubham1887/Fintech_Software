using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.News
{
    public class UpdateRequest
    {
        private string _Title;

        public string Title
        {
            get => _Title.TrimIfNotNull();
            set => _Title = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _NewsID;

        public int NewsID
        {
            get => _NewsID;
            set => _NewsID = value;
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _HyperLink;

        public string HyperLink
        {
            get => _HyperLink.TrimIfNotNull();
            set => _HyperLink = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.Title)
                      .Must(new Validation().IsValidString)
                      .WithMessage(NewsMessage.InvalidTitle);

            RuleFor(x => x.Description)
                      .Must(new Validation().IsValidString)
                      .WithMessage(NewsMessage.InvalidDescription);

            RuleFor(x => x.NewsID)
                     .Must(new Validation().IsValidInt)
                     .WithMessage(NewsMessage.InvalidNewsID);
        }
    }
}