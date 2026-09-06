using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.MessageSetting
{
    public class UpdateRequest
    {
        private int _MessageSettingID;

        public int MessageSettingID
        {
            get => _MessageSettingID;
            set => _MessageSettingID = value;
        }

        private bool _IsSMS;

        public bool IsSMS
        {
            get => _IsSMS;
            set => _IsSMS = value;
        }

        public bool IsMail
        {
            get => _IsMail;
            set => _IsMail = value;
        }

        private bool _IsMail;

        private string _SmsMessage;

        public string SmsMessage
        {
            get => _SmsMessage.TrimIfNotNull();
            set => _SmsMessage = value.TrimIfNotNull();
        }

        private string _EmailMessage;

        public string EmailMessage
        {
            get => _EmailMessage.TrimIfNotNull();
            set => _EmailMessage = value.TrimIfNotNull();
        }

        private string _TemplateID;

        public string TemplateID
        {
            get => _TemplateID.TrimIfNotNull();
            set => _TemplateID = value.TrimIfNotNull();
        }

        private string _WhatsUpMessage;

        public string WhatsUpMessage
        {
            get => _WhatsUpMessage.TrimIfNotNull();
            set => _WhatsUpMessage = value.TrimIfNotNull();
        }

        private bool _IsWhatsUp;

        public bool IsWhatsUp
        {
            get => _IsWhatsUp;
            set => _IsWhatsUp = value;
        }

        private bool _IsOTP;

        public bool IsOTP
        {
            get => _IsOTP;
            set => _IsOTP = value;
        }

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
            RuleFor(x => x.MessageSettingID)
              .Must(new Validation().IsValidInt)
              .WithMessage(MessageSettingMessage.InvalidID);
        }
    }
}