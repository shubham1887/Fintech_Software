using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CreditInstitution.CreditInstitutionRequest
{
    public class CreditInstituteRequest
    {
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }


        internal string IsValidModalState()
        {
            var validator = new CreditInstituteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class CreditInstituteRequestValidator : AbstractValidator<CreditInstituteRequest>
    {
        public CreditInstituteRequestValidator()
        {
            
        }
    }
}