using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.Sender
{
    public class SelectRequest
    {
        private string _DMTName;

        [DefaultValue("")]
        public string DMTName
        {
            get => _DMTName.TrimIfNotNull();
            set => _DMTName = value.TrimIfNotNull();
        }

        private string _Mobile;

        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _Pipe;

        public string Pipe
        {
            get => _Pipe.TrimIfNotNull();
            set => _Pipe = value.TrimIfNotNull();
        }

        private int _ProviderID;

        internal int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _SenderID;

        public string SenderID
        {
            get => _SenderID.TrimIfNotNull();
            set => _SenderID = value.TrimIfNotNull();
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

    internal class SelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.Mobile)
               .Must(new Validation().IsValidMobileNumber)
               .WithMessage(SenderMessage.InvalidMobile);
        }
    }
}