using Project.Processing;
using System.Collections.Generic;

namespace Project.Requests.SignUp
{
    public class CreateProfileAPIRequest
    {
        public string reference_id { get; set; }
        public Config config { get; set; }
        public Data data { get; set; }
        public CreateProfileAPIRequest()
        {
            data = new Data();
            config = new Config(); // Initialize Config to avoid null reference
               
        }
    }
    public class Data
    {
        public indPanDetails ind_pan_details { get; set; }
        public List<AdditionalDetails> additional_details { get; set; }

        public Data()
        {
            ind_pan_details = new indPanDetails();
            additional_details = new List<AdditionalDetails>();
        }
    }
    public class Config
    {
        public string id { get; set; }
    }
    public class indPanDetails
    {
        public string id_number { get; set; }
    }
    public class AdditionalDetails
    {
        public string category { get; set; }
        public string value { get; set; }
        public string page { get; set; }
        public string type { get; set; }
    }
}