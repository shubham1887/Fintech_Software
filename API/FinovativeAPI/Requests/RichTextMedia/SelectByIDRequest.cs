using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.RichTextMedia
{
    public class SelectByIDRequest
    {
        private int _ID;

        public int ID
        {
            get => _ID;
            set => _ID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.ID)
                .Must(new Validation().IsValidInt)
                .WithMessage(BusSettingMasterMessages.InvalidID);
        }
    }
}