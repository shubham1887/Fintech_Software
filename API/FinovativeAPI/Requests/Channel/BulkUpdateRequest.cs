using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Channel
{
    public class BulkUpdateRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID; set => _ServiceID = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private string _CommissionType;

        public string CommissionType
        {
            get => _CommissionType.TrimIfNotNull();
            set => _CommissionType = value.TrimIfNotNull();
        }

        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory.TrimIfNotNull();
            set => _CommissionCategory = value.TrimIfNotNull();
        }

        private string _SlabType;

        public string SlabType
        {
            get => _SlabType.TrimIfNotNull();
            set => _SlabType = value.TrimIfNotNull();
        }

        private int _ChannelSlabMasterID;

        public int ChannelSlabMasterID
        {
            get => _ChannelSlabMasterID;
            set => _ChannelSlabMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new BulkUpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return (!success) ? failures[0].ToString() : StatusType.Ok;
        }
    }

    internal class BulkUpdateRequestValidator : AbstractValidator<BulkUpdateRequest>
    {
        public BulkUpdateRequestValidator()
        {
            RuleFor(x => x.ServiceID)
             .Must(new Validation().IsValidInt)
             .WithMessage(OperatorMessage.InvalidServiceID);
        }
    }
}