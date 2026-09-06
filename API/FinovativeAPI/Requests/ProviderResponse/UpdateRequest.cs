using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderResponse
{
    public class UpdateRequest
    {
        private int _ProviderResponseMasterID;

        private int _ServiceID;
        private string _Type;

        private int _ProviderID;

        private string _Status;

        public string TextMustExists
        {
            get => _TextMustExists.TrimIfNotNull();
            set => _TextMustExists = value.TrimIfNotNull();
        }

        public string Response
        {
            get =>
                _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        public bool IsRefCode
        {
            get => _IsRefCode;
            set => _IsRefCode = value;
        }

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        public int ProviderResponseMasterID
        {
            get => _ProviderResponseMasterID;
            set => _ProviderResponseMasterID = value;
        }

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        public string Type
        {
            get => _Type;
            set => _Type = value;
        }

        public string Status
        {
            get => _Status;
            set => _Status = value;
        }

        private string _TextMustExists;
        private string _Response;

        private bool _IsActive;

        private bool _IsRefCode;

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
            RuleFor(x => x.ProviderID)
             .Must(new Validation().IsValidInt)
             .WithMessage(ProviderResponseMessage.InvalidProvider);

            RuleFor(x => x.TextMustExists)
             .Must(new Validation().IsValidString)
             .WithMessage(ProviderResponseMessage.InvalidTextMustExists);

            RuleFor(x => x.Type)
                .Must(new Validation().IsValidStringWithoutZero)
                .WithMessage(ProviderResponseMessage.InvalidType);

            RuleFor(x => x.Response)
                .Must(new Validation().IsValidString)
                .WithMessage(ProviderResponseMessage.InvalidResponse);

            RuleFor(x => x.Status)
                 .Must(new Validation().IsValidStringWithoutZero)
                 .WithMessage(ProviderResponseMessage.InvalidStatus);
        }
    }
}