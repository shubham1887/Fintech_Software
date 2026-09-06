using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderConfigurationAdminCommission
{
    public class UpdateRequest
    {
        private decimal _AdminComm;

        public decimal AdminComm
        {
            get => _AdminComm;
            set => _AdminComm = value;
        }

        private string _AdminCommType;

        public string AdminCommType
        {
            get => _AdminCommType.TrimIfNotNull();
            set => _AdminCommType = value.TrimIfNotNull();
        }

        private bool _AdminIsSurcharge;

        public bool AdminIsSurcharge
        {
            get => _AdminIsSurcharge;
            set => _AdminIsSurcharge = value;
        }

        private int _ProviderConfigurationAdminCommissionID;

        public int ProviderConfigurationAdminCommissionID
        {
            get => _ProviderConfigurationAdminCommissionID;
            set => _ProviderConfigurationAdminCommissionID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (!success)
            {
                return failures[0].ToString();
            }
            //success = IsValidFromToAmount();
            //if (!success)
            //{
            //    return SlabDetailMessage.ValidAmount;
            //}

            return StatusType.Ok;
        }
    }

    public class UpdateRequestRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestRequestValidator()
        {
            RuleFor(x => x.AdminComm)
             .Must(new Validation().IsValidDecimalWithNigative)
             .WithMessage(SlabDetailMessage.InvalidAmout);

            RuleFor(x => x.AdminCommType)
            .Must(new Validation().IsValidCommissionTypeString)
            .WithMessage(SlabDetailMessage.InvalidCommssionType);
        }
    }
}