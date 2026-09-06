using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OfflineBookingDetails
{
    public class InsertRequest
    {
        private int _OfflineProductMasterID;

        public int OfflineProductMasterID
        {
            get => _OfflineProductMasterID;
            set => _OfflineProductMasterID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _Through;

        public string Through
        {
            get => _Through;
            set => _Through = value;
        }

        private decimal _Amount;

        internal decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.TrimIfNotNull();
            set => _PersonName = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
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
            RuleFor(x => x.OfflineProductMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(OfflineBookingMessage.InvalidProduct);

            RuleFor(x => x.ConsumerNumber)
            .Must(new Validation().IsValidString)
            .WithMessage(OfflineBookingMessage.InvalidConsumerNumber);

            RuleFor(x => x.PersonName)
          .Must(new Validation().IsValidString)
          .WithMessage(PrepaidActivationMessage.InvalidName);

            RuleFor(x => x.Address)
             .Must(new Validation().IsValidString)
             .WithMessage(PrepaidActivationMessage.InvalidAddress);

            RuleFor(x => x.Pincode)
             .Must(new Validation().IsValidString)
             .WithMessage(PrepaidActivationMessage.InvalidPincode);
        }
    }
}