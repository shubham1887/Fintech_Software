using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.LeadGenerationMaster
{
    public class UpdateRequest
    {
        private int _LeadGenerationMasterID;

        public int LeadGenerationMasterID
        {
            get => _LeadGenerationMasterID;
            set => _LeadGenerationMasterID = value;
        }
        private int _LoanTypeMasterID;

        public int LoanTypeMasterID
        {
            get => _LoanTypeMasterID;
            set => _LoanTypeMasterID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
        
        private string _LoanType;

        public string LoanType
        {
            get => _LoanType.TrimIfNotNull();
            set => _LoanType = value.TrimIfNotNull();
        }
        
        private string _ApprovedAmount;

        public string ApprovedAmount
        {
            get => _ApprovedAmount.TrimIfNotNull();
            set => _ApprovedAmount = value.TrimIfNotNull();
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

    public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            /*RuleFor(x => x.FormID)
                .Must(new Validation().IsValidString)
                .WithMessage(LeadGenerationMasterMessage.InvalidFormID);
*/
        }
    }
}