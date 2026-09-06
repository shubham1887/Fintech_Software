using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.LeadGenerationMaster
{
    public class InsertRequest
    {

        private DateTime _FromDate;

        public DateTime FromDate
        {
            get => _FromDate;
            set => _FromDate = value;
        }
        
        private string _Gender;

        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }

        private string _FormID;

        public string FormID
        {
            get => _FormID.TrimIfNotNull();
            set => _FormID = value.TrimIfNotNull();
        }
        
        private string _MerchantName;

        public string MerchantName
        {
            get => _MerchantName.TrimIfNotNull();
            set => _MerchantName = value.TrimIfNotNull();
        }
        
        private string _RetailerName;

        public string RetailerName
        {
            get => _RetailerName.TrimIfNotNull();
            set => _RetailerName = value.TrimIfNotNull();
        }
        
        private string _ConsumerName;

        public string ConsumerName
        {
            get => _ConsumerName.TrimIfNotNull();
            set => _ConsumerName = value.TrimIfNotNull();
        }
        
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }
        
        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }
        
        private string _AadharNumber;

        public string AadharNumber
        {
            get => _AadharNumber.TrimIfNotNull();
            set => _AadharNumber = value.TrimIfNotNull();
        }
        
        private string _PanNumber;

        public string PanNumber
        {
            get => _PanNumber.TrimIfNotNull();
            set => _PanNumber = value.TrimIfNotNull();
        }
        
        private string _ConsumerType;

        public string ConsumerType
        {
            get => _ConsumerType.TrimIfNotNull();
            set => _ConsumerType = value.TrimIfNotNull();
        }
        
        private string _RequiredAmount;

        public string RequiredAmount
        {
            get => _RequiredAmount.TrimIfNotNull();
            set => _RequiredAmount = value.TrimIfNotNull();
        }
        
        private string _LoanType;

        public string LoanType
        {
            get => _LoanType.TrimIfNotNull();
            set => _LoanType = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
        
        private string _AIPRequestUserID;

        public string AIPRequestUserID
        {
            get => _AIPRequestUserID.TrimIfNotNull();
            set => _AIPRequestUserID = value.TrimIfNotNull();
        }
        
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }


        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.PanNumber)
                .Must(new Validation().IsValidPanCard)
                .WithMessage(LeadGenerationMasterMessage.InvalidPanNumber);

            RuleFor(x => x.AadharNumber)
                .Must(new Validation().IsValidAadharNo)
                .WithMessage(LeadGenerationMasterMessage.InvalidAadharNumber);
            
            RuleFor(x => x.RequiredAmount)
                .Must(new Validation().IsValidString)
                .WithMessage(LeadGenerationMasterMessage.InvalidRequiredAmount);
            
            RuleFor(x => x.MobileNumber)
                .Must(new Validation().IsValidString)
                .WithMessage(LeadGenerationMasterMessage.InvalidMobileNumber);
            
            RuleFor(x => x.ConsumerName)
                .Must(new Validation().IsValidString)
                .WithMessage(LeadGenerationMasterMessage.InvalidConsumerName);

        }
    }
}