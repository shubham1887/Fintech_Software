using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OfflineSlabDetail
{
    public class SelectBySlabMasterIDRequest
    {
        private int _SlabMasterID;

        public int SlabMasterID
        {
            get => _SlabMasterID;
            set => _SlabMasterID = value;
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

        private string _ServiceType;

        public string ServiceType
        {
            get => _ServiceType.TrimIfNotNullWithDefault(StaticModel.ServiceType.Recharge);
            set => _ServiceType = value.TrimIfNotNullWithDefault(StaticModel.ServiceType.Recharge);
        }

        internal string IsValidModalState()
        {
            var validator = new SelectBySlabMasterIDRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectBySlabMasterIDRequestValidator : AbstractValidator<SelectBySlabMasterIDRequest>
    {
        public SelectBySlabMasterIDRequestValidator()
        {
            RuleFor(x => x.SlabMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}