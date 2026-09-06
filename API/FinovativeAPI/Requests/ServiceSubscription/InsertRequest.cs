using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.ServiceSubscription
{
    public class InsertRequest
    {
        public CustomFile Document = new CustomFile();

        private int _ServiceSubscriptionCreatePlanID;

        public int ServiceSubscriptionCreatePlanID
        {
            get => _ServiceSubscriptionCreatePlanID;
            set => _ServiceSubscriptionCreatePlanID = value;
        }

        private string _PlanName;

        public string PlanName
        {
            get => _PlanName.TrimIfNotNull();
            set => _PlanName = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private List<ServiceList> _list;

        public List<ServiceList> Services
        {
            get => _list;
            set => _list = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        internal string IsValidModalStateForDocument()
        {
            var validator = new FileValidator();
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
            RuleFor(x => x.ServiceSubscriptionCreatePlanID)
                .Must(new Validation().IsValidInt)
                .WithMessage(AEPSMessage.InvalidID);
        }
    }
    internal class FileValidator : AbstractValidator<InsertRequest>
    {
        public FileValidator()
        {
            RuleFor(x => x.Document.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.Document.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);
        }
    }
}