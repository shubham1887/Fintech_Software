using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PaymentGateway
{
    public class UpdateRequest
    {
        private int _PaymentGatewaySettingID;

        public int PaymentGatewaySettingID
        {
            get => _PaymentGatewaySettingID;
            set => _PaymentGatewaySettingID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _MerchantID;

        public string MerchantID
        {
            get => _MerchantID.TrimIfNotNull();
            set => _MerchantID = value.TrimIfNotNull();
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }

        private string _Slat;

        public string Slat
        {
            get => _Slat.TrimIfNotNull();
            set => _Slat = value.TrimIfNotNull();
        }

        private string _SuccessURL;

        public string SuccessURL
        {
            get => _SuccessURL.TrimIfNotNull();
            set => _SuccessURL = value.TrimIfNotNull();
        }

        private string _FailURL;

        public string FailURL
        {
            get => _FailURL.TrimIfNotNull();
            set => _FailURL = value.TrimIfNotNull();
        }

        private string _ActionURL;

        public string ActionURL
        {
            get => _ActionURL.TrimIfNotNull();
            set => _ActionURL = value.TrimIfNotNull();
        }

        private string _StatusCheckURL;

        public string StatusCheckURL
        {
            get => _StatusCheckURL.TrimIfNotNull();
            set => _StatusCheckURL = value.TrimIfNotNull();
        }

        private bool _IsActiveWeb;

        public bool IsActiveWeb
        {
            get => _IsActiveWeb;
            set => _IsActiveWeb = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private bool _IsUPI;

        public bool IsUPI
        {
            get => _IsUPI;
            set => _IsUPI = value;
        }

        private decimal _MinimumAmount;

        public decimal MinimumAmount
        {
            get => _MinimumAmount;
            set => _MinimumAmount = value;
        }

        private decimal _MaximumAmount;

        public decimal MaximumAmount
        {
            get => _MaximumAmount;
            set => _MaximumAmount = value;
        }

        private int _SettlementHoldInMinutes;
        public int SettlementHoldInMinutes
        {
            get => _SettlementHoldInMinutes;
            set => _SettlementHoldInMinutes = value;
        }
        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            if (success == false)
                return failures[0].ToString();

            if (MinimumAmount > MaximumAmount)
                return $"MinimumAmount should less than  {MaximumAmount}";

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.PaymentGatewaySettingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(PaymentGatewayMessage.InvalidID);
            RuleFor(x => x.Name)
              .Must(new Validation().IsValidString)
              .WithMessage(PaymentGatewayMessage.InvalidName);

            RuleFor(x => x.MerchantID)
           .Must(new Validation().IsValidString)
           .WithMessage(PaymentGatewayMessage.InvalidMerchantID);

            RuleFor(x => x.Slat)
           .Must(new Validation().IsValidString)
           .WithMessage(PaymentGatewayMessage.InvalidSlat);

            RuleFor(x => x.Token)
           .Must(new Validation().IsValidString)
           .WithMessage(PaymentGatewayMessage.InvalidToken);

            RuleFor(x => x.SuccessURL)
           .Must(new Validation().IsValidString)
           .WithMessage(PaymentGatewayMessage.InvalidSuccessURL);

            RuleFor(x => x.FailURL)
           .Must(new Validation().IsValidString)
           .WithMessage(PaymentGatewayMessage.InvalidFailURL);
        }
    }
}