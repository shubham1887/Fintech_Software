using Processing.Processing;
using Project.Model;
using Project.Processing;
using Project.StaticModel;

namespace Project.BAL
{
    public class OnboardingCallbackBal
    {
        public string OnboardingUserInsert(Project.Requests.Onboarding.OnboardingDataRequest request)
        {
            string failureMessage = string.Empty;

            if (request.config == null)
            {
                failureMessage = Message.OnboardingMessages.DataNotFound;
                return failureMessage;
            }
            

            Project.Requests.Onboarding.OnboardingRequest onboarding = new Project.Requests.Onboarding.OnboardingRequest();
            onboarding.ID = request.config.id;
            onboarding.ProfileID = request.profile_id;
            onboarding.OrderID = request.reference_id;

            foreach (var task in request.tasks)
            {
                if (task.result.manual_response != null)
                {
                    onboarding.LOB_details = task.result?.manual_response?.value ?? "";
                    if (task.key.ToLower().Contains("bank_verification"))
                    {
                        onboarding.BeneficiaryName = task.result?.manual_response?.name_at_bank ?? "";
                        onboarding.account_exists = task.result?.manual_response?.account_exists ?? "";
                    }

                    for (int i = 1; i <= 10; i++)
                    {
                        string key = $"key{i}";
                        if (task.key.ToLower().Contains(key.ToLower()))
                        {
                            if (task.result?.manual_response?.value != string.Empty)
                            {
                                onboarding.Type_of_Shop = task.result?.manual_response?.value ?? "";
                                break;
                            } 
                        }
                    }
              
                    if (task.key.ToLower().Contains("gst_6fe5"))
                    {
                      
                        onboarding.trade_name = task.result?.manual_response?.extraction_output?.trade_name ?? "";
                        onboarding.pan_number = task.result?.manual_response?.extraction_output?.pan_number ?? "";
                    }
                    if (task.key.ToLower().Contains("key_verify_gst_00d9"))
                    {
                        var principalPlaceOfBusiness = task.result.manual_response?.source_output?.principal_place_of_business_fields?.principal_place_of_business_address;


                        onboarding.GST_Address = $"{principalPlaceOfBusiness?.door_number ?? ""}, {principalPlaceOfBusiness?.street ?? ""}, {principalPlaceOfBusiness?.location ?? ""}, {principalPlaceOfBusiness?.dst ?? ""}, {principalPlaceOfBusiness?.state_name ?? ""}, {principalPlaceOfBusiness?.pincode ?? ""}";


                    }

                }
                if (task.result?.automated_response?.result?.xml_output != null)
                {
                    onboarding.DOB = task.result?.automated_response?.result?.xml_output?.dob ?? "";
                    onboarding.FullName = task.result?.automated_response?.result?.xml_output?.name ?? "";
                    onboarding.Gender = task.result?.automated_response?.result?.xml_output?.gender ?? "";
                    onboarding.Residential_Address = task.result?.automated_response?.result?.xml_output?.street_address ?? "";
                    onboarding.Residential_Locality = task.result?.automated_response?.result?.xml_output?.address?.vtc ?? "";
                    onboarding.Residential_District = task.result?.automated_response?.result?.xml_output?.address?.dist ?? "";
                    onboarding.State = task.result?.automated_response?.result?.xml_output?.address?.state ?? "";
                    onboarding.Country = task.result?.automated_response?.result?.xml_output?.address?.country ?? "";
                    onboarding.AadhaarNo = task.result?.automated_response?.result?.xml_output?.uid ?? "";
                }
                
            }
            foreach (var responseData in request.resources.text)
            {
                if (responseData.ref_id.ToLower().Contains("bank_account_number"))
                {
                    onboarding.AccountNumber = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("bank_ifsc_code"))
                {
                    onboarding.IFSC = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("name_at_bank"))
                {
                    onboarding.BeneficiaryName = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("addess"))
                {
                    onboarding.Address = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("email_id"))
                {
                    onboarding.EmailID = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("nil.nil.name.1"))
                {
                    onboarding.FirmName = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("poi.ind_pan.id_number"))
                {
                    onboarding.PanNo = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("pincode"))
                {
                    onboarding.PinCode = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("mobile_number"))
                {
                    onboarding.MobileNumber = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("employee_code"))
                {
                    onboarding.ReferenceID = responseData.value.ToString();
                }

                if (responseData.ref_id.ToLower().Contains("addess"))
                {
                    onboarding.Business_Address = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("lob_details"))
                {
                    onboarding.LineOfBusiness = responseData.value.ToString();
                }

                if (responseData.ref_id.ToLower().Contains("handicapped"))
                {
                    onboarding.Handicapped = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("occupation"))
                {
                    onboarding.Occupation = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("education1"))
                {
                    onboarding.Highest_Qualification = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("start_time"))
                {
                    onboarding.start_time = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("end_time"))
                {
                    onboarding.end_time = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("category"))
                {
                    onboarding.Category = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("week"))
                {
                    onboarding.Working_Days = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("referral_code"))
                {
                    onboarding.Referral_Code = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("employee_code"))
                {
                    onboarding.Employee_code = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("gst"))
                {
                    onboarding.GstNo = responseData.value.ToString();
                }
                if (responseData.ref_id.ToLower().Contains("shop1"))
                {
                    onboarding.Address_Proof = responseData.value.ToString();
                }
                if (responseData.attr != null)
                {
                    if (responseData.attr.ToLower().EqualsIgnoreCase("address"))
                    {
                        onboarding.Address_Proof_Document_Number = responseData.value.ToString();
                    }
                }

            }
            if (!request.reviewer_action.ToLower().EqualsIgnoreCase("approved"))
            {
                failureMessage = new SignUpBal().UpdateStatus(onboarding.MobileNumber, request.reference_id, request.profile_id, request.reviewer_action);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return failureMessage;
                }
                return failureMessage;
            }

            foreach (var DocumentUpload in request.resources.images)
            {
                Model.OnboardingUserDocument document = new Model.OnboardingUserDocument();
                string DocumentName = GetDocumentNameFromRefId(DocumentUpload.ref_id);
                document.Ref_ID = DocumentName;
                string path = new MediaProcessing().SaveImageIdfyOnboarding(DocumentUpload.value, DocumentName, onboarding.FullName);
                if (path != string.Empty)
                {

                    document.NewImage = path;
                    onboarding.onboardingUserDocuments.Add(document);
                }
                if (DocumentUpload.ref_id.ToLower().Contains("shop"))
                {
                    onboarding.Business_Locality = DocumentUpload?.location?.city ?? "";
                    onboarding.Business_District = DocumentUpload?.location?.city ?? "";
                    onboarding.Business_Pincode = DocumentUpload?.location?.pincode ?? "";
                    onboarding.Business_State = DocumentUpload?.location?.state ?? "";
                    onboarding.Latitude = DocumentUpload?.location?.latitude ?? "";
                    onboarding.Longitude = DocumentUpload?.location?.longitude ?? "";
                }
            }

            foreach (var DocumentUpload in request.resources.documents)
            {
                Model.OnboardingUserDocument document = new Model.OnboardingUserDocument();

                string path = new MediaProcessing().SavePdfIdfyOnboarding(DocumentUpload.value, DocumentUpload.type, onboarding.FullName);
                if (path != string.Empty)
                {
                    string DocumentName = GetDocumentNameFromRefId(DocumentUpload.ref_id);
                    document.Ref_ID = DocumentName;
                    document.NewImage = path;

                    onboarding.onboardingUserDocuments.Add(document);

                }
            }
       




            string Result = new OnboardingUserBAL(). Insert(onboarding);
            if (!Result.EqualsIgnoreCase(StaticModel.StatusType.Ok))
            {
                return Result;
            }
            failureMessage = StaticModel.StatusType.Ok;
            return failureMessage;
        }
        private string GetDocumentNameFromRefId(string refId)
        {
            if (refId.Contains("establishment"))
            {
                return refId.EndsWith(".1.1") ? "Establishment1" : "Establishment2";
            }
            if (refId.Contains("shop_images"))
            {
                return refId.EndsWith(".1.1") ? "Shop_images1" : "Shop_images2";
            }
            if (refId.Contains("bank_cheque"))
            {
                return "Cheque";
            }
            if (refId.Contains("selfie"))
            {
                return "Selfie";
            }
            if (refId.Contains("ind_pan"))
            {
                return "PAN";
            }
            if (refId.Contains("profile_report"))
            {
                return "Profile_Report";
            }
            if (refId.Contains("ind_aadhaar"))
            {
                return "Aadhaar";
            }
            if (refId.Contains("gst_certificate"))
            {
                return "GST";
            }
            return "unknown"; 
        }
    }
    
}
