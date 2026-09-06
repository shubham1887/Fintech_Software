using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Provider
{
    public class ProviderDetailsInsertRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _ProviderURL;

        public string ProviderURL
        {
            get => _ProviderURL.TrimIfNotNull();
            set => _ProviderURL = value.TrimIfNotNull();
        }

        private string _MethodType;

        public string MethodType
        {
            get => _MethodType.TrimIfNotNull();
            set => _MethodType = value.TrimIfNotNull();
        }

        private string _HeaderJson;

        public string HeaderJson
        {
            get => _HeaderJson.TrimIfNotNull();
            set => _HeaderJson = value.TrimIfNotNull();
        }

        private string _DateFormat;

        public string DateFormat
        {
            get => _DateFormat.TrimIfNotNull();
            set => _DateFormat = value.TrimIfNotNull();
        }

        private string _UrlType;

        public string UrlType
        {
            get => _UrlType.TrimIfNotNull();
            set => _UrlType = value.TrimIfNotNull();
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.ToTitleCase();
            set => _ProviderName = value.ToTitleCase();
        }

        private int _UrlTypeID;

        public int UrlTypeID
        {
            get => _UrlTypeID;
            set => _UrlTypeID = value;
        }

        private int _ProviderDetailID;

        public int ProviderDetailID
        {
            get => _ProviderDetailID;
            set => _ProviderDetailID = value;
        }

        private string _TransactionIDPrefix;

        public string TransactionIDPrefix
        {
            get => _TransactionIDPrefix;
            set => _TransactionIDPrefix = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ProviderDetailsRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Provider details insert with :ProviderID:{ProviderID}, ServiceID:{ServiceID}, ProviderURL:{ProviderURL}";
        }
    }

    public class ProviderDetailsRequestValidator : AbstractValidator<ProviderDetailsInsertRequest>
    {
        public ProviderDetailsRequestValidator()
        {
            RuleFor(x => x.ServiceID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.ServiceMessage.Invalid);

            RuleFor(x => x.ProviderID)
              .Must(new Validation().IsValidInt)
              .WithMessage(Message.ServiceMessage.Invalid);

            RuleFor(x => x.MethodType)
              .Must(new Validation().IsValidString)
              .WithMessage(OperatorMessage.InValidOperator);

            RuleFor(x => x.ProviderURL)
                .Must(new Validation().IsValidString)
                .WithMessage(OperatorMessage.InValidOperator);

            RuleFor(x => x.UrlType)
                .Must(new Validation().IsValidString)
                .WithMessage(OperatorMessage.InValidOperator);
        }
    }
}