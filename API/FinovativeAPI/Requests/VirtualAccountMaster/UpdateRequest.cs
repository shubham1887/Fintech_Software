using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.VirtualAccountMaster
{
    public class UpdateRequest
    {
        private int _VirtualAccountMasterID;

        public int VirtualAccountMasterID
        {
            get => _VirtualAccountMasterID;
            set => _VirtualAccountMasterID = value;
        }

        private string _Prefix;

        public string Prefix
        {
            get => _Prefix.TrimIfNotNull();
            set => _Prefix = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
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
            RuleFor(x => x.VirtualAccountMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(VirtaulAccountMessage.InvalidID);
            RuleFor(x => x.BankName)
              .Must(new Validation().IsValidString)
              .WithMessage(VirtaulAccountMessage.InvalidBankName);

            RuleFor(x => x.Prefix)
             .Must(new Validation().IsValidString)
             .WithMessage(VirtaulAccountMessage.InvalidPrefix);

            RuleFor(x => x.IFSC)
             .Must(new Validation().IsValidString)
             .WithMessage(VirtaulAccountMessage.InvalidIFSC);
        }
    }
}