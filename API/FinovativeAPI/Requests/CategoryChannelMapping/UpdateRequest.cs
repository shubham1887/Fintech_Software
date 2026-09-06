using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CategoryChannelMapping
{
    public class UpdateRequest
    {
        public CustomFile ChannelImage = new CustomFile();

        private int _CategoryChannelMappingID;

        public int CategoryChannelMappingID
        {
            get => _CategoryChannelMappingID;
            set => _CategoryChannelMappingID = value;
        }

        private int _ChannelCategoryID;

        public int ChannelCategoryID
        {
            get => _ChannelCategoryID;
            set => _ChannelCategoryID = value;
        }

        private string _ChannelName;

        public string ChannelName
        {
            get => _ChannelName.TrimIfNotNull();
            set => _ChannelName = value.TrimIfNotNull();
        }

        private string _ChannelImagePath;

        public string ChannelImagePath
        {
            get => _ChannelImagePath.TrimIfNotNull();
            set => _ChannelImagePath = value.TrimIfNotNull();
        }

        internal string IsValidModalStateForDocument()
        {
            var validator = new UpadteFileValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
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

    public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.CategoryChannelMappingID)
         .Must(new Validation().IsValidInt)
         .WithMessage(CategoryChannelMessage.InValidID);
            RuleFor(x => x.ChannelCategoryID)
                .Must(new Validation().IsValidInt)
                .WithMessage(CategoryChannelMessage.InValidChannelCategoryID);

            RuleFor(x => x.ChannelName)
                .Must(new Validation().IsValidString)
                .WithMessage(CategoryChannelMessage.InvalidChannelName);
        }
    }

    internal class UpadteFileValidator : AbstractValidator<UpdateRequest>
    {
        public UpadteFileValidator()
        {
            RuleFor(x => x.ChannelImage.Size).NotEmpty()
                .Must(new Validation().IsValidFileOrImageSize)
                .WithMessage(Message.TicketMessage.InvalidFileSize);

            RuleFor(x => x.ChannelImage.Type).NotEmpty()
                .Must(new Validation().IsValidImageType)
                .WithMessage(Message.TicketMessage.InvalidFileType);
        }
    }
}