using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Complaints
{
    public class AddRequest
    {
        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _CustomerRemark;

        public string CustomerRemark
        {
            get => _CustomerRemark.TrimIfNotNull();
            set => _CustomerRemark = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _NewConsumerNumber;

        public string NewConsumerNumber
        {
            get => _NewConsumerNumber.TrimIfNotNull();
            set => _NewConsumerNumber = value.TrimIfNotNull();
        }

        private string _ComplaintType;

        public string ComplaintType
        {
            get => _ComplaintType;
            set => _ComplaintType = value;
        }

        public string Through { get; internal set; }

        internal string IsValidModalState()
        {
            var validator = new AddRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class AddRequestValidator : AbstractValidator<AddRequest>
    {
        public AddRequestValidator()
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