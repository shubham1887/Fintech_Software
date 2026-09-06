using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserType
{
    public class UpdateRequest
    {
        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private bool _IsOutside;

        public bool IsOutside
        {
            get => _IsOutside;
            set => _IsOutside = value;
        }

        private bool _IsMail;

        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class SelectRequestValidator : AbstractValidator<UpdateRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.UserTypeID)
              .Must(new Validation().IsValidInt)
              .WithMessage(MessageSettingMessage.InvalidID);
        }
    }
}