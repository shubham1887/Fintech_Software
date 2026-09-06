using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.VirtualAccountMaster
{
    public class SelectByIDRequest
    {
        private int _VirtualAccountMasterID;

        public int VirtualAccountMasterID
        {
            get => _VirtualAccountMasterID;
            set => _VirtualAccountMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.VirtualAccountMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(VirtaulAccountMessage.InvalidID);
        }
    }
}