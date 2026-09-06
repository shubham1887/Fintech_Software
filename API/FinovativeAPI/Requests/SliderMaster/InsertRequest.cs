using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SliderMaster
{
    public class InsertRequest
    {
        public CustomFile SliderImage = new CustomFile();

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
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

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.SliderImage.Data)
               .Must(new Validation().IsValidString)
               .WithMessage(Message.SliderMasterMessages.InvalidImage);

            RuleFor(x => x.Description)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.SliderMasterMessages.InvalidDescription);

            RuleFor(x => x.Type)
              .Must(new Validation().IsValidString)
               .WithMessage(Message.SliderMasterMessages.InvalidType);
        }
    }
}