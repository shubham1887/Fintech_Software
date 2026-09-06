using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Sequence
{
    public class InsertRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }

        private List<ProviderList> _List;

        public List<ProviderList> providerList
        {
            get => _List;
            set => _List = value;
        }

        private int _RealProviderID;

        public int RealProviderID
        {
            get => _RealProviderID;
            set => _RealProviderID = value;
        }

        private int _ViewProviderID;

        public int ViewProviderID
        {
            get => _ViewProviderID;
            set => _ViewProviderID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Sequence change for OperatorID:{OperatorID}, by UserID:{UserID}, with {providerList.ToString()}";
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SequenceMessage.InvalidService);
            RuleFor(x => x.OperatorID)
               .Must(new Validation().IsValidInt)
               .WithMessage(Message.SequenceMessage.InvalidOperator);
        }
    }
}