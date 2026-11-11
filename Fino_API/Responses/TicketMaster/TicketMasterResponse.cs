namespace FintechApp.Responses.TicketMaster
{
    public class TicketMasterResponse
    {
        public int TicketMasterID { get; set; }
        public int TicketCategoryID { get; set; }
        public string TicketCategoryName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string UploadFilePath { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public DateTime ResolutionDate { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int ResolutionByUserID { get; set; }
        public int UserTypeID { get; set; }
        public string UserTypeName { get; set; } = string.Empty;
        public bool IsDelete { get; set; }

        /// <summary>
        /// When the ticket was raised.
        ///
        /// It was never sent, so the queue's "Raised" column showed a dash on every row and the
        /// screen's date filter had nothing to filter on — a support queue you cannot sort or
        /// date-range is not a queue, it is a list.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>Last change — set when the ticket is answered or its status moves.</summary>
        public DateTime UpdatedateTime { get; set; }
    }
}