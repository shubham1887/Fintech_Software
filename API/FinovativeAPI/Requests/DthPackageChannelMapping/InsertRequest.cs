using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.Response.CategoryChannelMapping;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.DthPackageChannelMapping
{
    public class InsertRequest
    {
        private int _OfflineProductMasterID;

        public int OfflineProductMasterID
        {
            get => _OfflineProductMasterID;
            set => _OfflineProductMasterID = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private int _ChannelCategoryID;

        public int ChannelCategoryID
        {
            get => _ChannelCategoryID;
            set => _ChannelCategoryID = value;
        }

        private List<ViewModel> _CategoryChannelMappingList;

        public List<ViewModel> CategoryChannelMappingList
        {
            get => _CategoryChannelMappingList;
            set => _CategoryChannelMappingList = value;
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

    public class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.OfflineProductMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(CategoryChannelMessage.InValidOfflineProductMasterID);

            RuleFor(x => x.ChannelCategoryID)
                .Must(new Validation().IsValidInt)
                .WithMessage(CategoryChannelMessage.InValidChannelCategoryID);
        }
    }
}