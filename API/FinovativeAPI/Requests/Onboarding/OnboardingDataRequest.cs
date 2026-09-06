using System;
using System.Collections.Generic;

namespace Project.Requests.Onboarding
{
    public class OnboardingDataRequest
    {


            public Config config { get; set; }
            public Device_Info device_info { get; set; }
            public Profile_Data profile_data { get; set; }
            public string profile_id { get; set; }
            public string reference_id { get; set; }
            public Resources resources { get; set; }
            public string reviewer_action { get; set; }
            public string schema_version { get; set; }
            public string status { get; set; }
            public Status_Description status_description { get; set; }
            public object status_detail { get; set; }
            public Task[] tasks { get; set; }
            public string version { get; set; }
            public object tag { get; set; }
        

        public class Config
        {
            public string id { get; set; }
            public object overrides { get; set; }
        }

        public class Device_Info
        {
            public string final_ipv4 { get; set; }
            public string user_agent { get; set; }
        }

        public class Profile_Data
        {
            public DateTime completed_at { get; set; }
            public DateTime created_at { get; set; }
            public object[] email { get; set; }
            public object[] mobile_number { get; set; }
            public object notes { get; set; }
            public object[] performed_by { get; set; }
            public object purged_at { get; set; }
        }

        public class Resources
        {
            public Document[] documents { get; set; }
            public Image[] images { get; set; }
            public Text[] text { get; set; }
            public object[] videos { get; set; }
        }

        public class Document
        {
            public object attr { get; set; }
            public Location location { get; set; }
            public Metadata metadata { get; set; }
            public string ref_id { get; set; }
            public int source { get; set; }
            public object tags { get; set; }
            public string type { get; set; }
            public string value { get; set; }
        }

        public class Location
        {
        }

        public class Metadata
        {
            public string capture_mechanism { get; set; }
        }

        public class Image
        {
            public string attr { get; set; }
            public Location1 location { get; set; }
            public Metadata1 metadata { get; set; }
            public string ref_id { get; set; }
            public int source { get; set; }
            public object tags { get; set; }
            public string type { get; set; }
            public string value { get; set; }
        }

        public class Location1
        {
            public float accuracy { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public string country_code { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public string pincode { get; set; }
            public string state { get; set; }
            public string street_address { get; set; }
        }

        public class Metadata1
        {
            public string capture_mechanism { get; set; }
            public Location2 location { get; set; }
        }

        public class Location2
        {
            public float accuracy { get; set; }
            public float latitude { get; set; }
            public float longitude { get; set; }
        }

        public class Text
        {
            public string attr { get; set; }
            public Location3 location { get; set; }
            public Metadata2 metadata { get; set; }
            public string ref_id { get; set; }
            public int source { get; set; }
            public object tags { get; set; }
            public string type { get; set; }
            public object value { get; set; }
        }

        public class Location3
        {
        }

        public class Metadata2
        {
        }

        public class Status_Description
        {
        }

        public class Task
        {
            public string key { get; set; }
            public string[] resources { get; set; }
            public Result result { get; set; }
            public string status { get; set; }
            public string task_id { get; set; }
            public string task_type { get; set; }
            public string question { get; set; }
        }

        public class Result
        {
            public Automated_Response automated_response { get; set; }
            public Manual_Response manual_response { get; set; }
        }

        public class Automated_Response
        {
            public Image_1 image_1 { get; set; }
            public Image_2 image_2 { get; set; }
            public bool is_a_match { get; set; }
            public float match_score { get; set; }
            public bool review_recommended { get; set; }
            public Match_Output match_output { get; set; }
            public Source_Output source_output { get; set; }
            public Extraction_Output extraction_output { get; set; }
            public Result1 result { get; set; }
            public bool are_eyes_open { get; set; }
            public int confidence { get; set; }
            public Face_Box face_box { get; set; }
            public Face_Coverage face_coverage { get; set; }
            public bool face_detected { get; set; }
            public bool face_mask_detected { get; set; }
            public Face_Quality face_quality { get; set; }
            public bool is_face_cropped { get; set; }
            public bool is_front_facing { get; set; }
            public bool is_live { get; set; }
            public bool is_nsfw { get; set; }
            public bool multiple_faces_detected { get; set; }
            public bool review_required { get; set; }
            public bool sunglass_detected { get; set; }
        }

        public class Image_1
        {
            public bool face_detected { get; set; }
            public string face_quality { get; set; }
        }

        public class Image_2
        {
            public bool face_detected { get; set; }
            public string face_quality { get; set; }
        }

        public class Match_Output
        {
            public bool is_match { get; set; }
            public int match_score { get; set; }
        }

        public class Source_Output
        {
            public bool aadhaar_seeding_status { get; set; }
            public bool dob_match { get; set; }
            public Input_Details input_details { get; set; }
            public bool name_match { get; set; }
            public string pan_status { get; set; }
            public string status { get; set; }
            public Additional_Place_Of_Business_Fields additional_place_of_business_fields { get; set; }
            public string centre_jurisdiction { get; set; }
            public object centre_jurisdiction_code { get; set; }
            public string constitution_of_business { get; set; }
            public object date_of_cancellation { get; set; }
            public string date_of_registration { get; set; }
            public string gstin { get; set; }
            public string gstin_status { get; set; }
            public object last_updated_date { get; set; }
            public string legal_name { get; set; }
            public string[] nature_of_business_activity { get; set; }
            public Principal_Place_Of_Business_Fields principal_place_of_business_fields { get; set; }
            public object source { get; set; }
            public object state_jurisdiction_code { get; set; }
            public string taxpayer_type { get; set; }
            public string trade_name { get; set; }
        }

        public class Input_Details
        {
            public string input_dob { get; set; }
            public string input_name { get; set; }
            public string input_pan_number { get; set; }
        }

        public class Additional_Place_Of_Business_Fields
        {
        }

        public class Principal_Place_Of_Business_Fields
        {
            public string nature_of_principal_place_of_business { get; set; }
            public Principal_Place_Of_Business_Address principal_place_of_business_address { get; set; }
        }

        public class Principal_Place_Of_Business_Address
        {
            public object building_name { get; set; }
            public object city { get; set; }
            public string door_number { get; set; }
            public string dst { get; set; }
            public object floor_number { get; set; }
            public object latitude { get; set; }
            public string location { get; set; }
            public object longitude { get; set; }
            public string pincode { get; set; }
            public string state_name { get; set; }
            public string street { get; set; }
        }

        public class Extraction_Output
        {
            public int age { get; set; }
            public string date_of_birth { get; set; }
            public string date_of_issue { get; set; }
            public bool exact_age { get; set; }
            public string fathers_name { get; set; }
            public string id_number { get; set; }
            public bool is_scanned { get; set; }
            public bool minor { get; set; }
            public string name_on_card { get; set; }
            public string pan_type { get; set; }
            public string address { get; set; }
            public string constitution_of_business { get; set; }
            public string date_of_liability { get; set; }
            public string gstin { get; set; }
            public bool is_provisional { get; set; }
            public string legal_name { get; set; }
            public string pan_number { get; set; }
            public string trade_name { get; set; }
            public string type_of_registration { get; set; }
            public string valid_from { get; set; }
            public string valid_upto { get; set; }
        }

        public class Result1
        {
            public Xml_Output xml_output { get; set; }
        }

        public class Xml_Output
        {
            public Address address { get; set; }
            public int age { get; set; }
            public string dob { get; set; }
            public string doc_type { get; set; }
            public bool exact_age { get; set; }
            public string file_format { get; set; }
            public string gender { get; set; }
            public DateTime generated_at { get; set; }
            public string h { get; set; }
            public string lang { get; set; }
            public string name { get; set; }
            public string reference_id { get; set; }
            public string status { get; set; }
            public string street_address { get; set; }
            public string task_id { get; set; }
            public string uid { get; set; }
        }

        public class Address
        {
            public string co { get; set; }
            public string country { get; set; }
            public string dist { get; set; }
            public string house { get; set; }
            public string lm { get; set; }
            public string loc { get; set; }
            public string pc { get; set; }
            public string state { get; set; }
            public string street { get; set; }
            public string vtc { get; set; }
        }

        public class Face_Box
        {
            public int height { get; set; }
            public int left { get; set; }
            public int top { get; set; }
            public int width { get; set; }
        }

        public class Face_Coverage
        {
            public string message { get; set; }
            public int percentage { get; set; }
            public string status { get; set; }
        }

        public class Face_Quality
        {
            public string message { get; set; }
            public string status { get; set; }
        }

        public class Manual_Response
        {
            public object[] change_log { get; set; }
            public bool changed { get; set; }
            public Image_11 image_1 { get; set; }
            public Image_21 image_2 { get; set; }
            public bool is_a_match { get; set; }
            public float match_score { get; set; }
            public bool review_recommended { get; set; }
            public Match_Output1 match_output { get; set; }
            public Source_Output1 source_output { get; set; }
            public Extraction_Output1 extraction_output { get; set; }
            public string value { get; set; }
            public bool are_eyes_open { get; set; }
            public int confidence { get; set; }
            public Face_Box1 face_box { get; set; }
            public Face_Coverage1 face_coverage { get; set; }
            public bool face_detected { get; set; }
            public bool face_mask_detected { get; set; }
            public Face_Quality1 face_quality { get; set; }
            public bool is_face_cropped { get; set; }
            public bool is_front_facing { get; set; }
            public bool is_live { get; set; }
            public bool is_nsfw { get; set; }
            public bool multiple_faces_detected { get; set; }
            public bool review_required { get; set; }
            public bool sunglass_detected { get; set; }
            public string name_at_bank { get; set; }
            public string account_exists { get; set; }
            public string trade_name { get; set; }
            public string pan_number { get; set; }
        }
        
        public class Image_11
        {
            public bool face_detected { get; set; }
            public string face_quality { get; set; }
        }

        public class Image_21
        {
            public bool face_detected { get; set; }
            public string face_quality { get; set; }
        }

        public class Match_Output1
        {
            public bool is_match { get; set; }
            public int match_score { get; set; }
        }

        public class Source_Output1
        {
            public bool aadhaar_seeding_status { get; set; }
            public bool dob_match { get; set; }
            public Input_Details1 input_details { get; set; }
            public bool name_match { get; set; }
            public string pan_status { get; set; }
            public string status { get; set; }
            public Additional_Place_Of_Business_Fields1 additional_place_of_business_fields { get; set; }
            public string centre_jurisdiction { get; set; }
            public object centre_jurisdiction_code { get; set; }
            public string constitution_of_business { get; set; }
            public object date_of_cancellation { get; set; }
            public string date_of_registration { get; set; }
            public string gstin { get; set; }
            public string gstin_status { get; set; }
            public object last_updated_date { get; set; }
            public string legal_name { get; set; }
            public string[] nature_of_business_activity { get; set; }
            public Principal_Place_Of_Business_Fields1 principal_place_of_business_fields { get; set; }
            public object source { get; set; }
            public object state_jurisdiction_code { get; set; }
            public string taxpayer_type { get; set; }
            public string trade_name { get; set; }
        }

        public class Input_Details1
        {
            public string input_dob { get; set; }
            public string input_name { get; set; }
            public string input_pan_number { get; set; }
        }

        public class Additional_Place_Of_Business_Fields1
        {
        }

        public class Principal_Place_Of_Business_Fields1
        {
            public string nature_of_principal_place_of_business { get; set; }
            public Principal_Place_Of_Business_Address1 principal_place_of_business_address { get; set; }
        }

        public class Principal_Place_Of_Business_Address1
        {
            public object building_name { get; set; }
            public object city { get; set; }
            public string door_number { get; set; }
            public string dst { get; set; }
            public object floor_number { get; set; }
            public object latitude { get; set; }
            public string location { get; set; }
            public object longitude { get; set; }
            public string pincode { get; set; }
            public string state_name { get; set; }
            public string street { get; set; }
        }

        public class Extraction_Output1
        {
            public int age { get; set; }
            public string date_of_birth { get; set; }
            public string date_of_issue { get; set; }
            public bool exact_age { get; set; }
            public string fathers_name { get; set; }
            public string id_number { get; set; }
            public bool is_scanned { get; set; }
            public bool minor { get; set; }
            public string name_on_card { get; set; }
            public string pan_type { get; set; }
            public string address { get; set; }
            public string constitution_of_business { get; set; }
            public string date_of_liability { get; set; }
            public string gstin { get; set; }
            public bool is_provisional { get; set; }
            public string legal_name { get; set; }
            public string pan_number { get; set; }
            public string trade_name { get; set; }
            public string type_of_registration { get; set; }
            public string valid_from { get; set; }
            public string valid_upto { get; set; }
        }

        public class Face_Box1
        {
            public int height { get; set; }
            public int left { get; set; }
            public int top { get; set; }
            public int width { get; set; }
        }

        public class Face_Coverage1
        {
            public string message { get; set; }
            public int percentage { get; set; }
            public string status { get; set; }
        }

        public class Face_Quality1
        {
            public string message { get; set; }
            public string status { get; set; }
        }

    }
}

