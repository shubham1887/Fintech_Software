using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.EnquiryDetails
{
    public class InsertRequest
    {
        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.TrimIfNotNull();
            set => _PersonName = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _EnquiryType;

        public string EnquiryType
        {
            get => _EnquiryType.TrimIfNotNull();
            set => _EnquiryType = value.TrimIfNotNull();
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.PersonName)
                .Must(new Validation().IsValidString)
                .WithMessage(EnquiryDetailsMessage.Invalidname);
            RuleFor(x => x.MobileNumber)
               .Must(new Validation().IsValidString)
               .WithMessage(EnquiryDetailsMessage.InvalidMobile);
            RuleFor(x => x.EmailID)
             .Must(new Validation().IsValidString)
             .WithMessage(EnquiryDetailsMessage.InvalidEmailID);
            RuleFor(x => x.Address)
              .Must(new Validation().IsValidString)
              .WithMessage(EnquiryDetailsMessage.InvalidAddress);
            RuleFor(x => x.Remark)
             .Must(new Validation().IsValidString)
             .WithMessage(EnquiryDetailsMessage.InvalidRemark);
        }
    }
}