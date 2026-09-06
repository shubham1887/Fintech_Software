using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Provider
{
    public class ProviderDetailsUpdateRequest
    {
        private int _ProviderDetailID;

        public int ProviderDetailID
        {
            get => _ProviderDetailID;
            set => _ProviderDetailID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _ProviderURL;

        public string ProviderURL
        {
            get => _ProviderURL;
            set => _ProviderURL = value;
        }

        private string _MethodType;

        public string MethodType
        {
            get => _MethodType;
            set => _MethodType = value;
        }

        private string _HeaderJson;

        public string HeaderJson
        {
            get => _HeaderJson;
            set => _HeaderJson = value;
        }

        private string _DateFormat;

        public string DateFormat
        {
            get => _DateFormat.TrimIfNotNull();
            set => _DateFormat = value.TrimIfNotNull();
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _UrlType;

        public string UrlType
        {
            get => _UrlType;
            set => _UrlType = value;
        }

        private string _TransactionIDPrefix;

        public string TransactionIDPrefix
        {
            get => _TransactionIDPrefix;
            set => _TransactionIDPrefix = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ProviderDetailsUpdateRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Provider details update with :ProviderID:{ProviderID},ServiceID:{ServiceID},ProviderURL:{ProviderURL}";
        }
    }

    public class ProviderDetailsUpdateRequestValidator : AbstractValidator<ProviderDetailsUpdateRequest>
    {
        public ProviderDetailsUpdateRequestValidator()
        {
            RuleFor(x => x.ProviderDetailID)
              .Must(new Validation().IsValidInt)
              .WithMessage(ProviderMessage.InValidProviderDetails);
            RuleFor(x => x.ProviderID)
              .Must(new Validation().IsValidInt)
              .WithMessage(ProviderMessage.InValidProvider);
            RuleFor(x => x.ProviderURL)
             .Must(new Validation().IsValidString)
             .WithMessage(ProviderMessage.InValidProviderName);
            RuleFor(x => x.UrlType)
            .Must(new Validation().IsValidString)
            .WithMessage(ProviderMessage.InValidProviderURL);
        }
    }
}