using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.MenuMaster
{
    public class SelectByIDRequest
    {
        private int _MenuMasterID;

        public int MenuMasterID
        {
            get => _MenuMasterID;
            set => _MenuMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.MenuMasterID)
              .Must(new Validation().IsValidInt)
              .WithMessage(MenuMessages.InvalidMenu);
        }
    }
}