using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.BulkMessage
{
    public class InsertRequest
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private List<UserTypeList> _list;

        public List<UserTypeList> UserTypeList
        {
            get => _list;
            set => _list = value;
        }

        private bool _IsSMS;

        public bool IsSMS
        {
            get => _IsSMS;
            set => _IsSMS = value;
        }

        private bool _IsEmail;

        public bool IsEmail
        {
            get => _IsEmail;
            set => _IsEmail = value;
        }

        private bool _IsWhatsUp;

        public bool IsWhatsUp
        {
            get => _IsWhatsUp;
            set => _IsWhatsUp = value;
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
            RuleFor(x => x.Message)
                      .Must(new Validation().IsValidString)
                      .WithMessage(BulkMessageMessages.InvalidMessage);
        }
    }
}