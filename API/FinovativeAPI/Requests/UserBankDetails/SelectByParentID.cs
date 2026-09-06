using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserBankDetails
{
    public class SelectByParentID
    {
        private int _ParentID;

        public int ParentID { get => _ParentID; set => _ParentID = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectByParentIDValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByParentIDValidator : AbstractValidator<SelectByParentID>
    {
        public SelectByParentIDValidator()
        {
            RuleFor(x => x.ParentID)
           .Must(new Validation().IsValidInt)
           .WithMessage(UserBankDetailsMessage.InvalidID);
        }
    }
}