using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.LeadGeneration
{
    public class Paysprint_GenerateURLRequest
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _MobileNo;

        public string MobileNo
        {
            get => _MobileNo.TrimIfNotNull();
            set => _MobileNo = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _CustomerType;

        public string CustomerType
        {
            get => _CustomerType.TrimIfNotNull();
            set => _CustomerType = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _PinCode;

        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }

        private string _LandMark;

        public string LandMark
        {
            get => _LandMark.TrimIfNotNull();
            set => _LandMark = value.TrimIfNotNull();
        }

        private string _MerchantCode;

        public string MerchantCode
        {
            get => _MerchantCode.TrimIfNotNull();
            set => _MerchantCode = value.TrimIfNotNull();
        }

        private bool _Is_mobile_linked_aadhar;

        public bool Is_mobile_linked_aadhar
        {
            get => _Is_mobile_linked_aadhar;
            set => _Is_mobile_linked_aadhar = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _MonthlyIncome;

        public int MonthlyIncome
        {
            get => _MonthlyIncome;
            set => _MonthlyIncome = value;
        }

        private bool _IncomeProof;

        public bool IncomeProof
        {
            get => _IncomeProof;
            set => _IncomeProof = value;
        }

        private string _ActiveLeadGeneration;

        public string ActiveLeadGeneration
        {
            get => _ActiveLeadGeneration.TrimIfNotNull();
            set => _ActiveLeadGeneration = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new Paysprint_GenerateURLRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class Paysprint_GenerateURLRequestValidator : AbstractValidator<Paysprint_GenerateURLRequest>
    {
        public Paysprint_GenerateURLRequestValidator()
        {
            RuleFor(x => x.Name)
               .Must(new Validation().IsValidString)
               .WithMessage(PanMessage.InvalidFirmName);

            //RuleFor(x => x.MobileNo)
            //    .Must(new Validation().IsValidString)
            //    .WithMessage(PanMessage.InvalidLastName);

            RuleFor(x => x.EmailID)
               .Must(new Validation().IsValidString)
               .WithMessage(PanMessage.InvalidMode);
        }
    }
}