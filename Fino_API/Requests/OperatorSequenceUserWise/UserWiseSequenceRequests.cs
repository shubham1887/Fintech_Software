using FintechApp.Requests.OperatorSequence;

namespace FintechApp.Requests.OperatorSequenceUserWise
{
    public class SelectUserWiseSequenceRequest
    {
        public int UserID { get; set; }
        public int ServiceID { get; set; }
    }

    public class SelectUserWiseByOperatorRequest
    {
        public int UserID { get; set; }
        public int OperatorID { get; set; }
    }

    // Replaces the user's sequence for one operator (legacy OperatorSequenceUserWise/Insert).
    public class InsertUserWiseSequenceRequest
    {
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public List<SequenceProviderItem> ProviderList { get; set; } = new();
    }

    // Legacy DeleteByUserID payload — removes the user's sequence for one operator.
    public class DeleteUserWiseByUserRequest
    {
        public int UserID { get; set; }
        public int OperatorID { get; set; }
    }
}
