using FluentValidation;
using FluentValidation.Results;
using Project.BAL;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests
{
    public class ProviderUpdateRequest
    {
        private int _ProviderID;
        private string _ProviderName;
        private string _ProviderUrl;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        public string ProviderUrl
        {
            get => _ProviderUrl.TrimIfNotNull();
            set => _ProviderUrl = value.TrimIfNotNull();
        }

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _IPAddress;

        public string IPAddress
        {
            get => _IPAddress.TrimIfNotNull();
            set => _IPAddress = value.TrimIfNotNull();
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
            var validator = new ProviderUpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (!success)
            {
                return failures[0].ToString();
            }
            if (IsRealAPI)
                success = CheckSetDefultAndUserwiseSequence(ProviderID);

            if (!success)
                return ProviderMessage.Allredyset;

            return StatusType.Ok;
        }

        private bool CheckSetDefultAndUserwiseSequence(int ProviderID)
        {
            bool success = new OperatorSequenceBal().CheckSetDefultAndUserwiseSequence(ProviderID);
            return success;
        }

        public override string ToString()
        {
            return $"Provider Update with ProviderName:{ProviderName}";
        }
    }

    public class ProviderUpdateRequestValidator : AbstractValidator<ProviderUpdateRequest>
    {
        public ProviderUpdateRequestValidator()
        {
            RuleFor(x => x.ProviderID)
              .Must(new Validation().IsValidInt)
              .WithMessage(ProviderMessage.InValidProvider);

            RuleFor(x => x.ProviderName)
             .Must(new Validation().IsValidString)
             .WithMessage(ProviderMessage.InValidProviderName);

            RuleFor(x => x.ProviderUrl)
            .Must(new Validation().IsValidString)
            .WithMessage(ProviderMessage.InValidProviderURL);

            RuleFor(x => x.IPAddress)
           .Must(new Validation().IsValidString)
           .WithMessage(ProviderMessage.InValidIP);
        }
    }
}