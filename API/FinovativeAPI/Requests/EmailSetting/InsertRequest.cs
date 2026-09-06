using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.EmailSettingMaster
{
    public class InsertRequest
    {
        private string _Email;

        public string Email
        {
            get => _Email.TrimIfNotNull();
            set => _Email = value.TrimIfNotNull();
        }

        private string _Password;

        public string Password
        {
            get => _Password.TrimIfNotNull();
            set => _Password = value.TrimIfNotNull();
        }

        private string _HostName;

        public string HostName
        {
            get => _HostName.TrimIfNotNull();
            set => _HostName = value.TrimIfNotNull();
        }

        private int _Port;

        public int Port
        {
            get => _Port;
            set => _Port = value;
        }

        private bool _IsSSL;

        public bool IsSSL
        {
            get => _IsSSL;
            set => _IsSSL = false;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = false;
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.Email)
               .Must(new Validation().IsValidEmail)
               .WithMessage(Message.EmailSettingMessage.InvalidEmail);

            RuleFor(x => x.Password)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.EmailSettingMessage.InvalidPassword);

            RuleFor(x => x.HostName)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.EmailSettingMessage.InvalidHostName);

            RuleFor(x => x.Port)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.EmailSettingMessage.InvalidPort);
        }
    }
}