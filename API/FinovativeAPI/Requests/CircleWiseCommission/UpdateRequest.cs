using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CircleWiseCommission
{
    public class UpdateRequest
    {
        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
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

        private decimal _RetailerComm;

        public decimal RetailerComm
        {
            get => _RetailerComm;
            set => _RetailerComm = value;
        }

        private string _RetailerCommType;

        public string RetailerCommType
        {
            get => _RetailerCommType.TrimIfNotNullWithDefault("Fix");
            set => _RetailerCommType = value.TrimIfNotNullWithDefault("Fix");
        }

        private bool _RetailerIsFix;

        public bool RetailerIsFix
        {
            get => _RetailerIsFix;
            set => _RetailerIsFix = value;
        }

        private bool _RetailerIsSurcharge;

        public bool RetailerIsSurcharge
        {
            get => _RetailerIsSurcharge;
            set => _RetailerIsSurcharge = value;
        }

        private decimal _DistributorComm;

        public decimal DistributorComm
        {
            get => _DistributorComm;
            set => _DistributorComm = value;
        }

        private string _DistributorCommType;

        public string DistributorCommType
        {
            get => _DistributorCommType.TrimIfNotNullWithDefault("Fix");
            set => _DistributorCommType = value.TrimIfNotNullWithDefault("Fix");
        }

        private bool _DistributorIsFix;

        public bool DistributorIsFix
        {
            get => _DistributorIsFix;
            set => _DistributorIsFix = value;
        }

        private bool _DistributorIsSurcharge;

        public bool DistributorIsSurcharge
        {
            get => _DistributorIsSurcharge;
            set => _DistributorIsSurcharge = value;
        }

        private decimal _SuperDistributorComm;

        public decimal SuperDistributorComm
        {
            get => _SuperDistributorComm;
            set => _SuperDistributorComm = value;
        }

        private string _SuperDistributorCommType;

        public string SuperDistributorCommType
        {
            get => _SuperDistributorCommType.TrimIfNotNullWithDefault("Fix");
            set => _SuperDistributorCommType = value.TrimIfNotNullWithDefault("Fix");
        }

        private bool _SuperDistributorIsFix;

        public bool SuperDistributorIsFix
        {
            get => _SuperDistributorIsFix;
            set => _SuperDistributorIsFix = value;
        }

        private bool _SuperDistributorIsSurcharge;

        public bool SuperDistributorIsSurcharge
        {
            get => _SuperDistributorIsSurcharge;
            set => _SuperDistributorIsSurcharge = value;
        }

        private int _CircleWiseCommissionID;

        public int CircleWiseCommissionID
        {
            get => _CircleWiseCommissionID;
            set => _CircleWiseCommissionID = value;
        }

        private decimal _ClientComm;

        public decimal ClientComm
        {
            get => _ClientComm;
            set => _ClientComm = value;
        }

        private string _ClientCommType;

        public string ClientCommType
        {
            get => _ClientCommType.TrimIfNotNullWithDefault("Fix");
            set => _ClientCommType = value.TrimIfNotNullWithDefault("Fix");
        }

        private bool _ClientIsFix;

        public bool ClientIsFix
        {
            get => _ClientIsFix;
            set => _ClientIsFix = value;
        }

        private bool _ClientIsSurcharge;

        public bool ClientIsSurcharge
        {
            get => _ClientIsSurcharge;
            set => _ClientIsSurcharge = value;
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

        private bool IsValidFromToAmount()
        {
            bool success = new Validation().IsValidFromToAmount(FromAmount, ToAmount);
            return success;
        }
    }

    public class UpdateRequestRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestRequestValidator()
        {
            RuleFor(x => x.RetailerComm)
             .Must(new Validation().IsValidDecimalWithNigative)
             .WithMessage(SlabDetailMessage.InvalidAmout);

            RuleFor(x => x.RetailerCommType)
            .Must(new Validation().IsValidCommissionTypeString)
            .WithMessage(SlabDetailMessage.InvalidCommssionType);

            RuleFor(x => x.DistributorComm)
            .Must(new Validation().IsValidDecimalWithNigative)
            .WithMessage(SlabDetailMessage.InvalidAmout);

            RuleFor(x => x.DistributorCommType)
            .Must(new Validation().IsValidCommissionTypeString)
            .WithMessage(SlabDetailMessage.InvalidCommssionType);

            RuleFor(x => x.SuperDistributorComm)
            .Must(new Validation().IsValidDecimalWithNigative)
            .WithMessage(SlabDetailMessage.InvalidAmout);

            RuleFor(x => x.SuperDistributorCommType)
            .Must(new Validation().IsValidCommissionTypeString)
            .WithMessage(SlabDetailMessage.InvalidCommssionType);
        }
    }
}