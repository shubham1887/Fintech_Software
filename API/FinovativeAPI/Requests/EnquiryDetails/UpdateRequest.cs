using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.EnquiryDetails
{
    public class UpdateRequest
    {
        private string _AdminRemark;

        public string AdminRemark
        {
            get => _AdminRemark.TrimIfNotNull();
            set => _AdminRemark = value.TrimIfNotNull();
        }

        private int _EnquiryDetailsID;

        public int EnquiryDetailsID
        {
            get => _EnquiryDetailsID;
            set => _EnquiryDetailsID = value;
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
            RuleFor(x => x.AdminRemark)
              .Must(new Validation().IsValidString)
              .WithMessage(EnquiryDetailsMessage.InvalidAdminRemark);

            RuleFor(x => x.EnquiryDetailsID)
              .Must(new Validation().IsValidInt)
              .WithMessage(EnquiryDetailsMessage.InvalidID);
        }
    }
}