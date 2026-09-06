using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SupportUserAccessDetail
{
    public class UpdateRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private int _MenuMasterID;

        public int MenuMasterID
        {
            get => _MenuMasterID;
            set => _MenuMasterID = value;
        }

        private string _MenuGUID;

        public string MenuGUID
        {
            get => _MenuGUID.TrimIfNotNull();
            set => _MenuGUID = value.TrimIfNotNull();
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
            RuleFor(x => x.UserID)
           .Must(new Validation().IsValidInt)
           .WithMessage(UserMessage.InvalidUserID);
            RuleFor(x => x.MenuMasterID)
          .Must(new Validation().IsValidInt)
          .WithMessage(SupportUserMessage.InvalidMenuMasterID);
        }
    }
}