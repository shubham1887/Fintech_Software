using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SliderMaster
{
    public class UpdateRequest
    {
        public CustomFile SliderImage = new CustomFile();

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private int _SliderMasterID;

        public int SliderMasterID
        {
            get => _SliderMasterID;
            set => _SliderMasterID = value;
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
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

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.SliderMasterID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.SliderMasterMessages.InvalidSliderMasterID);

            RuleFor(x => x.Description)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.SliderMasterMessages.InvalidDescription);

            RuleFor(x => x.Type)
              .Must(new Validation().IsValidString)
              .WithMessage(Message.SliderMasterMessages.InvalidType);
        }
    }
}