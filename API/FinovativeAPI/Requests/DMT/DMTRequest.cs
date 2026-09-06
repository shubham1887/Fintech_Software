using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project.Requests.DMT
{
    public class DMTRequest
    {
        private string _DMTName;

        [DefaultValue("")]
        public string DMTName
        {
            get => _DMTName.TrimIfNotNull();
            set => _DMTName = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new DMTRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DMTRequestValidator : AbstractValidator<DMTRequest>
    {
        public DMTRequestValidator()
        {
        }
    }
}