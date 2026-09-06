using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Channel
{
    public class UpdateRequest
    {
        private int _ChannelSlabDetailID;

        public int ChannelSlabDetailID
        {
            get => _ChannelSlabDetailID;
            set => _ChannelSlabDetailID = value;
        }

        private string _CommissionType;

        public string CommissionType
        {
            get => _CommissionType.TrimIfNotNull();
            set => _CommissionType = value.TrimIfNotNull();
        }

        private decimal _Commission;
        public decimal Commission { get => _Commission; set => _Commission = value; }
        private string _RealCommissionType;

        public string RealCommissionType
        {
            get => _RealCommissionType;
            set => _RealCommissionType = value;
        }

        private decimal _RealCommission;

        public decimal RealCommission
        {
            get => _RealCommission;
            set => _RealCommission = value;
        }

        private string _GeneralCommissionCategory;

        public string GeneralCommissionCategory
        {
            get => _GeneralCommissionCategory.TrimIfNotNull();
            set => _GeneralCommissionCategory = value.TrimIfNotNull();
        }

        private string _RealCommissionCategory;

        public string RealCommissionCategory
        {
            get => _RealCommissionCategory.TrimIfNotNull();
            set => _RealCommissionCategory = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return (!success) ? failures[0].ToString() : StatusType.Ok;
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.CommissionType)
                .Must(new Validation().IsValidCommissionTypeString)
                .WithMessage(SlabDetailMessage.InvalidCommssionType);

            RuleFor(x => x.ChannelSlabDetailID)
             .Must(new Validation().IsValidInt)
             .WithMessage(SlabDetailMessage.InValidID);

            RuleFor(x => x.Commission)
            .Must(new Validation().IsValidDecimalWithNigative)
            .WithMessage(SlabDetailMessage.InvalidAmout);
        }
    }
}