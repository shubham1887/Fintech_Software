using FluentValidation;
using FluentValidation.Results;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.Requests
{
    public class KeyDataModelForAPIUser
    {
        private string _Mobile;

        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _APIToken;

        public string APIToken
        {
            get => _APIToken.TrimIfNotNull();
            set => _APIToken = value.TrimIfNotNull();
        }

        private int _UserID;

        internal int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        internal string IsValidModalState(string IPAddress)
        {
            var validator = new KeyDataModelForAPIUserValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;

            IList<ValidationFailure> failures = results.Errors;

            if (!success)
                return failures.FirstOrDefault().ToString();

            this.UserID = new UserBal().SelectUserIDByAPIToken(this.Mobile, this.APIToken);

            if (this.UserID <= 0)
                return UserMessage.InvalidMobileNumberAndToken;

            bool IsValidIPAddress = new IPMasterBal().IsValidIPAddress(IPAddress, this.UserID);

            if (!IsValidIPAddress)
                return UserMessage.RequestFromInvalidIP + IPAddress;

            return StatusType.Ok;
        }
    }

    public class KeyDataModelForAPIUserValidator : AbstractValidator<KeyDataModelForAPIUser>
    {
        public KeyDataModelForAPIUserValidator()
        {
            RuleFor(x => x.Mobile)
                .Must(new Validation().IsValidMobileNumber)
                .WithMessage(Message.UserMessage.InvalidUser);

            RuleFor(x => x.APIToken)
                .Must(new Validation().IsValidString)
                .WithMessage(Messages.InvalidAPIToken);
        }
    }
}