using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AgentDetails
{
    public class UploadRequest
    {
        public CustomFile ExcelDocument = new CustomFile();

        private string _ServiceType;
        public string ServiceType
        {
            get => _ServiceType.TrimIfNotNull();
            set => _ServiceType = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UploadRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UploadRequestValidator : AbstractValidator<UploadRequest>
    {
        public UploadRequestValidator()
        {

        }
    }
}