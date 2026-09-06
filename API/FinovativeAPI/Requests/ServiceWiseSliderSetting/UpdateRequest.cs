using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.Requests.ServiceWiseSliderSetting
{
    public class UpdateRequest
    {
        public CustomFile SliderImage = new CustomFile();
        private int _ServiceWiseSliderSettingID;

        public int ServiceWiseSliderSettingID
        {
            get => _ServiceWiseSliderSettingID;
            set => _ServiceWiseSliderSettingID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _Priority;

        public int Priority
        {
            get => _Priority;
            set => _Priority = value;
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _ReferenceLink;

        public string ReferenceLink
        {
            get => _ReferenceLink.TrimIfNotNull();
            set => _ReferenceLink = value.TrimIfNotNull();
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

            return success ? StatusType.Ok : failures.FirstOrDefault().ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        private readonly Validation validation = new Validation();

        public UpdateRequestValidator()
        {
            RuleFor(x => x.ServiceWiseSliderSettingID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(Message.ServiceWiseSliderSettingMessage.InvalidSlider);

            RuleFor(x => x.ServiceID)
               .Must(validation.IsValidInt)
               .WithMessage(Message.ServiceWiseSliderSettingMessage.InvalidService);

            RuleFor(x => x.SliderImage.Size).NotEmpty()
               .Must(new Validation().IsValidFileOrImageSize)
               .WithMessage(Message.ServiceWiseSliderSettingMessage.InvalidFileSize);

            RuleFor(x => x.SliderImage.Data).NotEmpty()
              .Must(new Validation().IsValidFileSize)
              .WithMessage(Message.ServiceWiseSliderSettingMessage.InvalidFileSize);

            RuleFor(x => x.SliderImage.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.ServiceWiseSliderSettingMessage.InvalidFileType);
        }
    }
}