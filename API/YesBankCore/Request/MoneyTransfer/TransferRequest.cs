using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    public class TransferRequest
    {
        private string _TxnId;

        public string TxnId
        {
            get => _TxnId.TrimIfNotNull();
            set => _TxnId = value.TrimIfNotNull();
        }

        private string _CustomerMob;

        public string CustomerMob
        {
            get => _CustomerMob.TrimIfNotNull();
            set => _CustomerMob = value.TrimIfNotNull();
        }

        private string _AgentId;

        public string AgentId
        {
            get => _AgentId.TrimIfNotNull();
            set => _AgentId = value.TrimIfNotNull();
        }

        private string _PartnerId;

        public string PartnerId
        {
            get => _PartnerId.TrimIfNotNull();
            set => _PartnerId = value.TrimIfNotNull();
        }

        private string _OrderId;

        public string OrderId
        {
            get => _OrderId.TrimIfNotNull();
            set => _OrderId = value.TrimIfNotNull();
        }

        private string _Channel;

        public string Channel
        {
            get => _Channel.TrimIfNotNull();
            set => _Channel = value.TrimIfNotNull();
        }

        //public List<string> ProductCode { get; set; }

        public string ProductCode = "DMT";

        private string _PurposeCode;

        public string PurposeCode
        {
            get => _PurposeCode.TrimIfNotNull();
            set => _PurposeCode = value.TrimIfNotNull();
        }

        private string _PaymentMode;

        public string PaymentMode
        {
            get => _PaymentMode.TrimIfNotNull();
            set => _PaymentMode = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _CurrencyCode;

        public string CurrencyCode
        {
            get => _CurrencyCode.TrimIfNotNull();
            set => _CurrencyCode = value.TrimIfNotNull();
        }

        private string _AccountNo;

        public string AccountNo
        {
            get => _AccountNo.TrimIfNotNull();
            set => _AccountNo = value.TrimIfNotNull();
        }

        private string _Ifsc;

        public string Ifsc
        {
            get => _Ifsc.TrimIfNotNull();
            set => _Ifsc = value.TrimIfNotNull();
        }

        private string _MobileNo;

        public string MobileNo
        {
            get => _MobileNo.TrimIfNotNull();
            set => _MobileNo = value.TrimIfNotNull();
        }

        private string _EmailId;

        public string EmailId
        {
            get => _EmailId.TrimIfNotNull();
            set => _EmailId = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Narration;

        public string Narration
        {
            get => _Narration.TrimIfNotNull();
            set => _Narration = value.TrimIfNotNull();
        }

        private string _HashKey;

        public string HashKey
        {
            get => _HashKey.TrimIfNotNull();
            set => _HashKey = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new TransferRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class TransferRequestValidator : AbstractValidator<TransferRequest>
    {
        public TransferRequestValidator()
        {
            RuleFor(x => x.TxnId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "txnId"));

            RuleFor(x => x.CustomerMob)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "customerMob"));

            RuleFor(x => x.AgentId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "agentId"));

            RuleFor(x => x.PartnerId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "partnerId"));

            RuleFor(x => x.OrderId)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "orderId"));

           // RuleFor(x => x.ProductCode)
           //.Must(new Validation().IsValidString)
           //.WithMessage(Message.ParameterMissing.Replace("[Param]", "productCode"));

            RuleFor(x => x.Amount)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "amount"));

            RuleFor(x => x.PaymentMode)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "paymentMode"));

            RuleFor(x => x.CurrencyCode)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "currencyCode"));

            RuleFor(x => x.Channel)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "channel"));       

            RuleFor(x => x.AccountNo)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "beneAccountNo"));

            RuleFor(x => x.Ifsc)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "beneIfsc"));

            RuleFor(x => x.Name)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "beneName"));
        }
    }
}