using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.PaymentGatewayLog
{
    public class SelectPaymentGatewayLogRequest
    {
        private string _FromDate;

        public string FromDate
        {
            get => _FromDate.TrimIfNotNull();
            set => _FromDate = value.TrimIfNotNull();
        }

        private string _SearchText;

        public string SearchText
        {
            get => _SearchText.TrimIfNotNull();
            set => _SearchText = value.TrimIfNotNull();
        }

        private string _ToDate;

        public string ToDate
        {
            get => _ToDate.TrimIfNotNull();
            set => _ToDate = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _GatewayName;

        public string GatewayName
        {
            get => _GatewayName.TrimIfNotNull();
            set => _GatewayName = value.TrimIfNotNull();
        }

        private int _TakeCount;

        public int TakeCount
        {
            get => _TakeCount;
            set => _TakeCount = value;
        }

        private int _SkipCount;

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
        }

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectPaymentGatewayLogRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectPaymentGatewayLogRequestValidator : AbstractValidator<SelectPaymentGatewayLogRequest>
    {
        public SelectPaymentGatewayLogRequestValidator()
        {
            RuleFor(x => x.FromDate)
                .Must(new Validation().IsValidString)
                .WithMessage(LogMessage.InvalidDate);

            RuleFor(x => x.ToDate)
               .Must(new Validation().IsValidString)
               .WithMessage(LogMessage.InvalidDate);
        }
    }
}