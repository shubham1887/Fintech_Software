using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Complaints
{
    public class AddRequestForAPIUser
    {
        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
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

        internal string IsValidModalState()
        {
            var validator = new AddRequestAPIUserValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class AddRequestAPIUserValidator : AbstractValidator<AddRequestForAPIUser>
    {
        public AddRequestAPIUserValidator()
        {
            RuleFor(x => x.ConsumerNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.TransactionMessage.InvalidConsumerNumber);

            RuleFor(x => x.APIUserRequestID)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ComplaintMessage.InvalidCustomerRemark);
        }
    }
}