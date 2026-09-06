using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserCreationSetting
{
    public class DeleteRequest
    {
        private int _UserCreationSettingID;

        public int UserCreationSettingID
        {
            get => _UserCreationSettingID;
            set => _UserCreationSettingID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.UserCreationSettingID)
                  .Must(new Validation().IsValidInt)
                  .WithMessage(UserCreationMessage.InvalidID);
        }
    }
}