using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.VoucherMaster
{
    public class SelectByIDForVouchertRequest
    {

        private int _VoucherMasterID;

        public int VoucherMasterID
        {
            get => _VoucherMasterID;
            set => _VoucherMasterID = value;
        }
        internal string IsValidModalState()
        {
            var validator = new SelectByIDForVouchertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByIDForVouchertRequestValidator : AbstractValidator<SelectByIDForVouchertRequest>
    {
        public SelectByIDForVouchertRequestValidator()
        {
             RuleFor(x => x.VoucherMasterID)
              .Must(new Validation().IsValidInt)
             .WithMessage(Project.Message.WalletMessage.Invalid);
        }
    }
}
