using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Channel
{
    public class InsertRequest
    {
        private string _CommissionType;

        public string CommissionType
        {
            get => _CommissionType.TrimIfNotNull();
            set => _CommissionType = value.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private int _SlabDeatilByChannelID;

        public int SlabDeatilByChannelID
        {
            get => _SlabDeatilByChannelID;
            set => _SlabDeatilByChannelID = value;
        }

        private decimal _FromAmount;

        public decimal FromAmount
        {
            get => _FromAmount;
            set => _FromAmount = value;
        }

        private decimal _ToAmount;

        public decimal ToAmount
        {
            get => _ToAmount;
            set => _ToAmount = value;
        }

        private int _ChannelSlabMasterID;

        public int ChannelSlabMasterID
        {
            get => _ChannelSlabMasterID;
            set => _ChannelSlabMasterID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private decimal _RealCommission;

        public decimal RealCommission
        {
            get => _RealCommission;
            set => _RealCommission = value;
        }

        private string _RealCommissionType;

        public string RealCommissionType
        {
            get => _RealCommissionType.TrimIfNotNull();
            set => _RealCommissionType = value.TrimIfNotNull();
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
            get => _RealCommissionCategory;
            set => _RealCommissionCategory = value;
        }

        public CommissionDetailWrapper DownlineCommission = new CommissionDetailWrapper();

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (!success)
            {
                return failures[0].ToString();
            }
            success = IsValidFromToAmount();
            if (!success)

                return SlabDetailMessage.ValidAmount;

            return StatusType.Ok;
        }

        private bool IsValidFromToAmount()
        {
            bool success = new Validation().IsValidFromToAmount(FromAmount, ToAmount);
            return success;
        }
    }

    public class CommissionDetailWrapper
    {
        public CommissionDetailModel General = new CommissionDetailModel();
        public CommissionDetailModel Real = new CommissionDetailModel();
    }

    public class CommissionDetailModel
    {
        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private string _CommissionType;

        public string CommissionType
        {
            get => _CommissionType.TrimIfNotNullWithDefault(Project.StaticModel.CommissionType.Fix);
            set => _CommissionType = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionType.Fix);
        }

        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
            set => _CommissionCategory = value.TrimIfNotNullWithDefault(Project.StaticModel.CommissionCategory.Commission);
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.ChannelSlabMasterID)
         .Must(new Validation().IsValidInt)
         .WithMessage(SlabMessage.InvalidName);

            RuleFor(x => x.FromAmount)
            .Must(new Validation().IsValidDecimal)
            .WithMessage(SlabDetailMessage.InvalidAmout);

            RuleFor(x => x.ToAmount)

                .Must(new Validation().IsValidDecimal)
                .WithMessage(SlabDetailMessage.InvalidAmout);

            RuleFor(x => x.OperatorID)
             .Must(new Validation().IsValidInt)
             .WithMessage(SlabDetailMessage.InvalidOperator);
        }
    }
}