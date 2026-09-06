using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.RazorPayQR
{
    public class CreateQRRequest
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _PANNumber;

        public string PANNumber
        {
            get => _PANNumber.TrimIfNotNull();
            set => _PANNumber = value.TrimIfNotNull();
        }

        private string _VPA;

        public string VPA
        {
            get => _VPA.TrimIfNotNull();
            set => _VPA = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new CreateQRRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            if (!success)
            {
                return failures[0].ToString();
            }
            //if (!VPA.ToLower().EndsWith("@icici".ToLower()))
            //{
            //    return "VPA should be ICICI";
            //}

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class CreateQRRequestValidator : AbstractValidator<CreateQRRequest>
    {
        public CreateQRRequestValidator()
        {
            RuleFor(x => x.FirmName)
           .Must(new Validation().IsValidString)
           .WithMessage(UPICollectioMessages.InvalidFirmName);

            RuleFor(x => x.PANNumber)
           .Must(new Validation().IsValidPanCard)
           .WithMessage(UPICollectioMessages.IsValidPanCard);

            RuleFor(x => x.VPA)
           .Must(new Validation().IsValidString)
           .WithMessage(UPICollectioMessages.InvalidVPA);
        }
    }
}