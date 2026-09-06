using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.VoucherMaster
{
    public class UpdateRequest
    {
        private int _VoucherMasterID;

        public int VoucherMasterID
        {
            get => _VoucherMasterID;
            set => _VoucherMasterID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
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
            RuleFor(x => x.VoucherMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(VoucherMessage.InValidVoucherMasterID);
            RuleFor(x => x.Status)
              .Must(new Validation().IsValidString)
              .WithMessage(VoucherMessage.InvalidStatus);
        }
    }
}