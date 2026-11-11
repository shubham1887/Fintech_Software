namespace FintechApp.Messages
{
    public static class ComplaintMasterMessages
    {
        public const string NotFound = "Complaint not found.";
        public const string CreatedSuccessfully = "Complaint created successfully.";
        public const string UpdatedSuccessfully = "Complaint updated successfully.";
        public const string DeletedSuccessfully = "Complaint deleted successfully.";
        public const string CancelledSuccessfully = "Complaint withdrawn.";

        // Cancel is deliberately narrow: it is the action a retailer gets INSTEAD of accept/reject,
        // so it may only ever touch their own still-open complaint.
        public const string CancelNotYours = "You can only withdraw a complaint you raised.";
        public const string AlreadyDecided = "This complaint has already been decided and can no longer be changed.";
        public const string ResolveNotAllowed = "Only an administrator or support user can accept or reject a complaint.";
    }
}
