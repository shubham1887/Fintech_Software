using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Provider
{
    public class ProviderMasterInsertRequest
    {
        private string _ProviderName;
        private string _ProviderUrl;

        public string ProviderName
        {
            get => _ProviderName;
            set => _ProviderName = value;
        }

        public string ProviderUrl
        {
            get => _ProviderUrl;
            set => _ProviderUrl = value;
        }

        private string _IPAddress;

        public string IPAddress
        {
            get => _IPAddress;
            set => _IPAddress = value;
        }

        private int _CopyFromProviderID;

        public int CopyFromProviderID
        {
            get { return _CopyFromProviderID; }
            set { _CopyFromProviderID = value; }
        }

        private bool _IsRealAPI;

        public bool IsRealAPI
        {
            get => _IsRealAPI;
            set => _IsRealAPI = value;
        }

        private string _LoginID;

        public string LoginID
        {
            get => _LoginID.TrimIfNotNull();
            set => _LoginID = value.TrimIfNotNull();
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }

        private string _SupportNumber;

        public string SupportNumber
        {
            get => _SupportNumber.TrimIfNotNull();
            set => _SupportNumber = value.TrimIfNotNull();
        }

        private int _LowBalanceAmount;

        public int LowBalanceAmount
        {
            get => _LowBalanceAmount;
            set => _LowBalanceAmount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new ProviderRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Provider insert with ProviderName:{ProviderName}";
        }
    }

    public class ProviderRequestValidator : AbstractValidator<ProviderMasterInsertRequest>
    {
        public ProviderRequestValidator()
        {
            RuleFor(x => x.ProviderName)
                .Must(new Validation().IsValidString)
                .WithMessage(OperatorMessage.InValidOperator);

            RuleFor(x => x.ProviderUrl)
               .Must(new Validation().IsValidString)
               .WithMessage(ProviderMessage.InValidProviderURL);

            RuleFor(x => x.IPAddress)
               .Must(new Validation().IsValidString)
               .WithMessage(ProviderMessage.InValidIP);
        }
    }
}