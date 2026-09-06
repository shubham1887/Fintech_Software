using System.Collections.Generic;

namespace Project.Response.AEPS
{
    public class BaseResponse
    {
        public int CorrelationId { get; set; }
        public bool Success { get; set; }
        public List<string> Messages { get; set; }
    }
}