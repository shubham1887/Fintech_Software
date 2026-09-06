using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PlanSetting
{
    public class UpdateRequest
    {
        private int _PlanSettingID;

        public int PlanSettingID
        {
            get => _PlanSettingID;
            set => _PlanSettingID = value;
        }

        private string _UserID;

        public string UserID
        {
            get => _UserID.TrimIfNotNull();
            set => _UserID = value.TrimIfNotNull();
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }
        
        private string _Password;

        public string Password
        {
            get => _Password.TrimIfNotNull();
            set => _Password = value.TrimIfNotNull();
        }

        private bool _IsActiveWeb;

        public bool IsActiveWeb
        {
            get => _IsActiveWeb;
            set => _IsActiveWeb = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }
        
        private string _Circle;
        public string Circle
        {
            get => _Circle.TrimIfNotNull();
            set => _Circle = value.TrimIfNotNull();
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
            RuleFor(x => x.PlanSettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(PlanSettingMessage.InvalidID);

            RuleFor(x => x.UserID)
           .Must(new Validation().IsValidString)
           .WithMessage(PlanSettingMessage.InvalidUserID);

            RuleFor(x => x.Token)
           .Must(new Validation().IsValidString)
           .WithMessage(PlanSettingMessage.InvalidToken);
        }
    }
}