using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.TransactionReport
{
    public class SelectPendingTransactionWithFilterRequest
    {
        public int ProviderID { get; set; }
        public int OperatorID { get; set; }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _SenderNo;

        public string SenderNo
        {
            get => _SenderNo.TrimIfNotNull();
            set => _SenderNo = value.TrimIfNotNull();
        }

        private string _PaymentType;

        public string PaymentType
        {
            get => _PaymentType.TrimIfNotNull();
            set => _PaymentType = value.TrimIfNotNull();
        }

        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        private int _SkipCount;

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
        }

        private int _TakeCount;

        public int TakeCount
        {
            get => _TakeCount;
            set => _TakeCount = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectPendingTransactionWithFilterRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return (success) ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectPendingTransactionWithFilterRequestValidator : AbstractValidator<SelectPendingTransactionWithFilterRequest>
    {
        public SelectPendingTransactionWithFilterRequestValidator()
        {
            //RuleFor(x => x.ProviderID).Must(new Validation().IsValidInt).WithMessage(TransactionDetailMessage.InvalidProvider).When(x => x.ProviderID <= 0);

            //RuleFor(x => x.OperatorID).Must(new Validation().IsValidInt).WithMessage(OperatorMessage.InValidOperator).When(x => x.OperatorID <= 0);
            //RuleFor(x => x.ConsumerNumber).Must(new Validation().IsValidString).WithMessage(TransactionMessage.InvalidConsumerNumber).When(x => x.ConsumerNumber != "");
        }
    }
}