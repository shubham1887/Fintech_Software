using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Complaints
{
    public class UpdateRequest
    {
        private int _ComplaintMasterID;

        private int _TransactionMasterID;

        private string _ConsumerNumber;

        private string _CustomerRemark;
        private string _AdminRemark;

        private string _Status;
        private decimal _Amount;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        public string CustomerRemark
        {
            get => _CustomerRemark.TrimIfNotNull();
            set => _CustomerRemark = value.TrimIfNotNull();
        }

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        public int ComplaintMasterID
        {
            get => _ComplaintMasterID;
            set => _ComplaintMasterID = value;
        }

        public string AdminRemark
        {
            get => _AdminRemark.TrimIfNotNull();
            set => _AdminRemark = value.TrimIfNotNull();
        }

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _ComplaintType;

        public string ComplaintType
        {
            get => _ComplaintType;
            set => _ComplaintType = value;
        }

        private string _NewConsumerNumber;

        public string NewConsumerNumber
        {
            get => _NewConsumerNumber;
            set => _NewConsumerNumber = value;
        }

        private string _OpTxnID;

        public string OpTxnID
        {
            get => _OpTxnID.TrimIfNotNull();
            set => _OpTxnID = value.TrimIfNotNull();
        }

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
            RuleFor(x => x.ConsumerNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.TransactionMessage.InvalidConsumerNumber);

            RuleFor(x => x.Amount)
            .Must(new Validation().IsValidDecimal)
            .WithMessage(Message.OperatorMessage.InValidAmount);

            RuleFor(x => x.CustomerRemark)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ComplaintMessage.InvalidCustomerRemark);
        }
    }
}