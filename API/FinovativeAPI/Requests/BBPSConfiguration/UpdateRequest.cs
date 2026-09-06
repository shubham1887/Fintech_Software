using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.BBPSConfiguration
{
    public class UpdateRequest
    {
        private int _ConfigurationID;

        public int ConfigurationID
        {
            get => _ConfigurationID;
            set => _ConfigurationID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _Label;

        public string Label
        {
            get => _Label.TrimIfNotNull();
            set => _Label = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _PlaceHolder;

        public string PlaceHolder
        {
            get => _PlaceHolder.TrimIfNotNull();
            set => _PlaceHolder = value.TrimIfNotNull();
        }

        private string _ErrorMeeage;

        public string ErrorMeeage
        {
            get => _ErrorMeeage.TrimIfNotNull();
            set => _ErrorMeeage = value.TrimIfNotNull();
        }

        private bool _IsRequired;

        public bool IsRequired
        {
            get => _IsRequired;
            set => _IsRequired = value;
        }

        private bool _IsDisplay;

        public bool IsDisplay
        {
            get => _IsDisplay;
            set => _IsDisplay = value;
        }

        private int _Sequence;

        public int Sequence
        {
            get => _Sequence;
            set => _Sequence = value;
        }

        private string _ConfigList;

        public string ConfigList
        {
            get => _ConfigList.TrimIfNotNullWithDefault("[]");
            set => _ConfigList = value.TrimIfNotNullWithDefault("[]");
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(config => config.ConfigurationID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(BBPSConfigurationMessages.InvalidConfiguration);

            RuleFor(config => config.OperatorID)
                      .Must(new Validation().IsValidInt)
                      .WithMessage(BBPSConfigurationMessages.InvalidOperator);

            RuleFor(config => config.Label)
                     .Must(new Validation().IsValidString)
                     .WithMessage(BBPSConfigurationMessages.InvalidLabel);

            RuleFor(config => config.Type)
                      .Must(new Validation().IsValidString)
                      .WithMessage(BBPSConfigurationMessages.InvalidType);

            RuleFor(config => config.PlaceHolder)
                      .Must(new Validation().IsValidString)
                      .WithMessage(BBPSConfigurationMessages.InvalidPlaceHolder);

            RuleFor(config => config.Sequence)
                      .Must(new Validation().IsValidInt)
                      .WithMessage(BBPSConfigurationMessages.InvalidSequence);

            RuleFor(config => config.ErrorMeeage)
                     .Must(new Validation().IsValidString)
                     .WithMessage(BBPSConfigurationMessages.InvalidErrorMeeage)
                     .When(config => config.IsRequired == true);
        }
    }
}