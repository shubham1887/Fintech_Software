using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ProviderConfig
{
    public class UpdateRequest
    {
        private int _ConfigurationID;

        private string _OpCode;

        public string OpCode
        {
            get => _OpCode.TrimIfNotNull();
            set => _OpCode = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private string _Lapu;

        public string Lapu
        {
            get => _Lapu.TrimIfNotNull();
            set => _Lapu = value.TrimIfNotNull();
        }

        private decimal? _AdminCommi;

        public decimal? AdminCommi
        {
            get => _AdminCommi;
            set => _AdminCommi = value;
        }

        private string _Quantity;

        public string Quantity
        {
            get => _Quantity.TrimIfNotNull();
            set => _Quantity = value.TrimIfNotNull();
        }

        public int ConfigurationID
        {
            get => _ConfigurationID;
            set => _ConfigurationID = value;
        }

        private string _AdminCommType;

        public string AdminCommType
        {
            get => _AdminCommType.TrimIfNotNull();
            set => _AdminCommType = value.TrimIfNotNull();
        }

        private string _CommissionCategory;

        public string CommissionCategory
        {
            get => _CommissionCategory.TrimIfNotNull();
            set => _CommissionCategory = value.TrimIfNotNull();
        }

        private int _PendingCount;

        public int PendingCount
        {
            get => _PendingCount;
            set => _PendingCount = value;
        }

        private int _PendingInMinute;

        public int PendingInMinute
        {
            get => _PendingInMinute;
            set => _PendingInMinute = value;
        }

        private int _FailCount;

        public int FailCount
        {
            get => _FailCount;
            set => _FailCount = value;
        }

        private int _FailInMinute;

        public int FailInMinute
        {
            get => _FailInMinute;
            set => _FailInMinute = value;
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"API config Update with OperatorCode:{OpCode},AdminCommi:{AdminCommi},Type:{Type}";
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.ConfigurationID)
                     .Must(new Validation().IsValidInt)
                     .WithMessage(ProviderConfigMessage.InvalidConfigurationID);
            RuleFor(x => x.OpCode)
                      .Must(new Validation().IsValidString)
                      .WithMessage(ProviderConfigMessage.InvalidCode);
        }
    }
}