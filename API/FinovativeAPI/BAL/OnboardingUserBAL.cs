using Processing.Processing;
using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class OnboardingUserBAL
    {
        internal string Update(Project.Requests.Onboarding.OnboardingRequest request, int UserID)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
        
            OnboardingUser onboardingUser = new OnboardingUserDal().SelectByID(request.OnboardingUserID);
            if (onboardingUser == null)
            {
                failureMessage = Message.OnboardingMessages.DataNotFound;
                return failureMessage;
            }

            var changes = new List<string>();
            var oldValues = new List<string>();
            var newValues = new List<string>();

            void TrackChange<T>(string propertyName, T oldValue, T newValue)
            {
                if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
                {
                    changes.Add(propertyName);
                    oldValues.Add(oldValue?.ToString());
                    newValues.Add(newValue?.ToString());
                }
            }
            TrackChange(nameof(onboardingUser.Gender), onboardingUser.Gender, request.Gender);
            TrackChange(nameof(onboardingUser.DOB), onboardingUser.DOB, request.DOB);
            TrackChange(nameof(onboardingUser.PanNo), onboardingUser.PanNo, request.PanNo);
            TrackChange(nameof(onboardingUser.Residential_Address), onboardingUser.Residential_Address, request.Residential_Address);
            TrackChange(nameof(onboardingUser.Name), onboardingUser.Name, request.FullName);
            TrackChange(nameof(onboardingUser.GSTNo), onboardingUser.GSTNo, request.GstNo);
            TrackChange(nameof(onboardingUser.FirmName), onboardingUser.FirmName, request.FirmName);
            TrackChange(nameof(onboardingUser.PinCode), onboardingUser.PinCode, request.PinCode);
            TrackChange(nameof(onboardingUser.Country), onboardingUser.Country, request.Country);
            TrackChange(nameof(onboardingUser.EmailID), onboardingUser.EmailID, request.EmailID);
            TrackChange(nameof(onboardingUser.MobileNumber), onboardingUser.MobileNumber, request.MobileNumber);
            TrackChange(nameof(onboardingUser.AccountNumber), onboardingUser.AccountNumber, request.AccountNumber);
            TrackChange(nameof(onboardingUser.IFSC), onboardingUser.IFSC, request.IFSC);
            TrackChange(nameof(onboardingUser.BankName), onboardingUser.BankName, request.BankName);
            TrackChange(nameof(onboardingUser.AadhaarNo), onboardingUser.AadhaarNo, request.AadhaarNo);
            TrackChange(nameof(onboardingUser.Business_Address), onboardingUser.Business_Address, request.Business_Address);
            TrackChange(nameof(onboardingUser.Residential_Locality), onboardingUser.Residential_Locality, request.Residential_Locality);
            TrackChange(nameof(onboardingUser.Business_Locality), onboardingUser.Business_Locality, request.Business_Locality);
            TrackChange(nameof(onboardingUser.EmployeeID), onboardingUser.EmployeeID, request.EmployeeID);
            TrackChange(nameof(onboardingUser.Referral_Code), onboardingUser.Referral_Code, request.Referral_Code);
            TrackChange(nameof(onboardingUser.Residential_District), onboardingUser.Residential_District, request.Residential_District);
            TrackChange(nameof(onboardingUser.LineOfBusiness), onboardingUser.LineOfBusiness, request.LineOfBusiness);
            TrackChange(nameof(onboardingUser.Type_of_Shop), onboardingUser.Type_of_Shop, request.Type_of_Shop);
            TrackChange(nameof(onboardingUser.Business_District), onboardingUser.Business_District, request.Business_District);
            TrackChange(nameof(onboardingUser.Business_State), onboardingUser.Business_State, request.Business_State);
            TrackChange(nameof(onboardingUser.Business_Pincode), onboardingUser.Business_Pincode, request.Business_Pincode);
            TrackChange(nameof(onboardingUser.Category), onboardingUser.Category, request.Category);
            TrackChange(nameof(onboardingUser.Occupation), onboardingUser.Occupation, request.Occupation);
            TrackChange(nameof(onboardingUser.Working_Days), onboardingUser.Working_Days, request.Working_Days);
            TrackChange(nameof(onboardingUser.Operating_Time), onboardingUser.Operating_Time, request.Operating_Time);
            TrackChange(nameof(onboardingUser.IIBF_Certification), onboardingUser.IIBF_Certification, request.IIBF_Certification);
            TrackChange(nameof(onboardingUser.IIBF_Certification_Number), onboardingUser.IIBF_Certification_Number, request.IIBF_Certification_Number);



            onboardingUser.Gender = request.Gender;
            onboardingUser.DOB = request.DOB;
            onboardingUser.PanNo = request.PanNo;
            onboardingUser.Residential_Address = request.Residential_Address;
            onboardingUser.Name = request.FullName;
            onboardingUser.GSTNo = request.GstNo;
            onboardingUser.FirmName = request.FirmName;
            onboardingUser.PinCode = request.PinCode;
            onboardingUser.Country = request.Country;
            onboardingUser.EmailID = request.EmailID;
            onboardingUser.MobileNumber = request.MobileNumber;
            onboardingUser.AccountNumber = request.AccountNumber;
            onboardingUser.IFSC = request.IFSC;
            onboardingUser.BankName = request.BankName;
            onboardingUser.SlabID = request.SlabID;
            onboardingUser.AadhaarNo = request.AadhaarNo;
            onboardingUser.Business_Address = request.Business_Address;
            onboardingUser.Residential_Locality = request.Residential_Locality;
            onboardingUser.Business_Locality = request.Business_Locality;
            onboardingUser.SupportStatus = StaticModel.StatusType.Process;
            onboardingUser.AdminStatus = StaticModel.StatusType.Process;
            onboardingUser.EmployeeID = request.EmployeeID;
            onboardingUser.Referral_Code = request.Referral_Code;
            onboardingUser.Gender = request.Gender;
            onboardingUser.Residential_District = request.Residential_District;
            onboardingUser.State = request.Residential_State;
            onboardingUser.LineOfBusiness = request.LineOfBusiness;
            onboardingUser.Type_of_Shop = request.Type_of_Shop;
            onboardingUser.Business_District = request.Business_District;
            onboardingUser.Business_State = request.Business_State;
            onboardingUser.Business_Pincode = request.Business_Pincode;
            onboardingUser.Category = request.Category;
            onboardingUser.Occupation = request.Occupation;
            onboardingUser.Working_Days = request.Working_Days;
            onboardingUser.Operating_Time = request.Operating_Time;
            onboardingUser.IIBF_Certification = request.IIBF_Certification;
            onboardingUser.IIBF_Certification_Number = request.IIBF_Certification_Number;
            onboardingUser.VerifiedPage = true;
            onboardingUser.supportUserVerify = false;
            onboardingUser.AdminVerify = false;

            int Result = new OnboardingUserDal().Update(onboardingUser);
            if (Result < 0)
            {
                failureMessage = Message.OnboardingMessages.UpdateFail;
                return failureMessage;
            }
            if (changes.Any())
            {
                foreach (var (change, oldValue, newValue) in changes.Zip(oldValues, newValues))
                {
                    LastChangeUserDetails userDetails = new LastChangeUserDetails();

                    userDetails.Change = change;
                    userDetails.OldChanges = oldValue;
                    userDetails.NewChanges = newValue;
                    userDetails.OnboardingUserID = onboardingUser.OnboardingUserID;
                    new LastChangeUserDetailsDal().Insert(userDetails);
                }
            }

            List<Model.OnboardingUserDocument> imagesRequest = new List<OnboardingUserDocument>();

            var imageProperties = new Dictionary<string, CustomFile>{
            {"Selfie", request.SelfieImage},
            {"Pan", request.PANImage},
            {"Aadhaar", request.AadhaarImage},
            {"GST", request.GSTImage},
            {"Establishment1", request.Establishment1Image},
            {"Establishment2", request.Establishment2Image},
            {"Shop_images1", request.Shop_images1Image},
            {"Shop_images2", request.Shop_images2Image},
            {"CVR_Report1", request.CVR_Report1},
            {"CVR_Report2", request.CVR_Report2}
            };


            foreach (var imagePair in imageProperties)
            {
                if (imagePair.Value != null && !string.IsNullOrEmpty(imagePair.Value.Name))
                {
                    string FileName = "";
                    string path = "";

                    Model.OnboardingUserDocument userDocument = new Model.OnboardingUserDocument();
                    userDocument = new OnboardingUserDocumentDal().SelectByOnboardingUserDocumentID(imagePair.Value.ID);
                    if (userDocument == null)
                    {
                        FileName = imagePair.Key;

                         path = new MediaProcessing().SaveImagesIdfy(imagePair.Value, MediaType.IdfyOnBoarding, FileName, onboardingUser.Name);
                        Model.OnboardingUserDocument UploadDocument = new Model.OnboardingUserDocument();
                        UploadDocument.OnboardingUserID = onboardingUser.OnboardingUserID;
                        UploadDocument.Ref_ID = imagePair.Key;
                        UploadDocument.NewImage = path;
                       
                            int InsertResult = new OnboardingUserDocumentDal().Insert(UploadDocument);
                        if (InsertResult < 0)
                            return Message.OnboardingMessages.InsertFailure;
                       
                    }
                    if (userDocument != null)
                    {
                        FileName = imagePair.Key + "_updateImage";

                         path = new MediaProcessing().SaveImagesIdfy(imagePair.Value, MediaType.IdfyOnBoarding, FileName, onboardingUser.Name);
                        userDocument.Ref_ID = imagePair.Key;
                        userDocument.NewImage = path;
                        userDocument.UpdatedDOC = DateTime.Now;

                        imagesRequest.Add(userDocument);

                        int result = new OnboardingUserDocumentDal().Update(userDocument);
                        if (Result < 0)
                            return Message.OnboardingMessages.InsertFailure;

                        LastChangeUserDetails userDetails = new LastChangeUserDetails();

                        userDetails.Change = userDocument.Ref_ID;
                        userDetails.OldChanges = userDocument.Ref_ID;
                        userDetails.NewChanges = userDocument.Ref_ID;
                        userDetails.OnboardingUserID = onboardingUser.OnboardingUserID;
                        new LastChangeUserDetailsDal().Insert(userDetails);
                    }


                }
            }
         
            failureMessage = StaticModel.StatusType.Ok;
            return failureMessage;
        }
        internal string Insert(Project.Requests.Onboarding.OnboardingRequest request)
        {
            OnboardingUser CheckDuplicate = new OnboardingUserDal().Duplicate(request.ID, request.ProfileID, request.PanNo);
            if (CheckDuplicate != null)
                return Message.OnboardingMessages.AlreadyDataExists;


            string failureMessage = string.Empty;
            OnboardingUser onboardingUser = new OnboardingUser();
            onboardingUser.OnboardingUserID = request.OnboardingUserID;
            onboardingUser.ProfileID = request.ProfileID;
            onboardingUser.OrderID = request.OrderID;
            onboardingUser.AadhaarNo = request.AadhaarNo;
            onboardingUser.ReferenceID = request.ReferenceID;
            onboardingUser.ID = request.ID;
            onboardingUser.Gender = request.Gender == "M" ? "Male" : "Female";
            onboardingUser.DOB = request.DOB;
            onboardingUser.PanNo = request.PanNo;
            onboardingUser.Name = request.FullName;
            onboardingUser.FirmName = request.FirmName;
            onboardingUser.PinCode = request.PinCode;
            onboardingUser.EmailID = request.EmailID;
            onboardingUser.MobileNumber = request.MobileNumber;
            onboardingUser.AccountNumber = request.AccountNumber;
            onboardingUser.IFSC = request.IFSC;
            onboardingUser.BankName = request.BankName;
            onboardingUser.Business_Address = request.Business_Address;
            onboardingUser.Residential_Address = request.Residential_Address;
            onboardingUser.Residential_Locality = request.Residential_Locality;
            onboardingUser.Business_Locality = request.Business_Locality;
            onboardingUser.Business_District = request.Business_District;
            onboardingUser.Business_Pincode = request.Business_Pincode;
            onboardingUser.Business_State = request.Business_State;
            onboardingUser.Latitude = request.Latitude;
            onboardingUser.Longitude = request.Longitude;
            onboardingUser.BeneficiaryName = request.BeneficiaryName;
            onboardingUser.Country = request.Country;
            onboardingUser.State = request.State;
            onboardingUser.Residential_District = request.Residential_District;
            onboardingUser.GSTNo = request.GstNo;
            onboardingUser.SupportStatus = StaticModel.StatusType.Process;
            onboardingUser.AdminStatus = StaticModel.StatusType.Process;
            onboardingUser.supportUserVerify = false;
            onboardingUser.AdminVerify = false;
            onboardingUser.CreateRole = (int)UserType.Retailer;
            onboardingUser.Working_Days = request.Working_Days;
            onboardingUser.LineOfBusiness = request.LineOfBusiness;
            onboardingUser.Handicapped = request.Handicapped;
            onboardingUser.Occupation = request.Occupation;
            onboardingUser.Highest_Qualification = request.Highest_Qualification;
            onboardingUser.Operating_Time = request.start_time + " TO " + request.end_time;
            onboardingUser.Referral_Code = request.Referral_Code;
            onboardingUser.EmployeeID = request.Employee_code;
            onboardingUser.Category = request.Category;
            onboardingUser.Type_of_Shop = request.Type_of_Shop;
            onboardingUser.pan_number = request.pan_number;
            onboardingUser.account_exists = request.account_exists;
            onboardingUser.trade_name = request.trade_name;
            onboardingUser.Address_Proof = request.Address_Proof;
            onboardingUser.Address_Proof_Document_Number = request.Address_Proof_Document_Number;
            onboardingUser.GST_Address = request.GST_Address;
            int Result = 0;
            try
            {
                Result = new OnboardingUserDal().Insert(onboardingUser);
            }
            catch (Exception e)
            {
                new Logger().Write(e);
            }

            if (Result < 0)
            {
                failureMessage = Message.OnboardingMessages.InsertFailure;
                return failureMessage;
            }
            failureMessage = OnboardingUserDocument(Result, request.onboardingUserDocuments);
            if (!failureMessage.EqualsIgnoreCase(StaticModel.StatusType.Ok))
            {
                return failureMessage;
            }
            if (request.Status.ToLower() == "approved")
            {
                request.Status = StaticModel.StatusType.Approve;
            }
            failureMessage = new SignUpBal().UpdateStatus(onboardingUser.MobileNumber, onboardingUser.OrderID, onboardingUser.ProfileID,request.Status);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;

            }
            failureMessage = StaticModel.StatusType.Ok;
            return failureMessage;
        }
        private string OnboardingUserDocument(int OnboardingUserID, List<Model.OnboardingUserDocument> requests)
        {
            string failureMessage = string.Empty;

            foreach (var item in requests)
            {
                Model.OnboardingUserDocument userDocument = new Model.OnboardingUserDocument();
                userDocument.OnboardingUserID = OnboardingUserID;
                userDocument.Ref_ID = item.Ref_ID;
                userDocument.NewImage = item.NewImage;
                int Result = 0;
                try
                {
                    Result = new OnboardingUserDocumentDal().Insert(userDocument);
                }
                catch (Exception e)
                {
                    new Logger().Write(e);
                }

                if (Result < 0)
                {
                    failureMessage = Message.OnboardingMessages.InsertFailure;
                    return failureMessage;
                }
            }

            failureMessage = StaticModel.StatusType.Ok;
            return failureMessage;
        }
        internal async Task<string> ApproveOnboarding(Project.Requests.Onboarding.ApproveOnboardingUserRequest request, int UserID)
        {
            string failureMessage = request.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            OnboardingUser onboardingUser = new OnboardingUserDal().SelectByID(request.OnboardingUserID);
            if (onboardingUser == null)
                return Message.OnboardingMessages.DataNotFound;

            if(onboardingUser.VerifiedPage != true)
                return Message.OnboardingMessages.VisitJourney;
            
            if (UserID != Company.AdminUserID)
            {
                onboardingUser.SupportStatus = request.Status;
                onboardingUser.SupportRemark = request.Remark;
                onboardingUser.supportUserID = UserID;
                onboardingUser.supportUserVerify = true;
            }

            if (UserID == Company.AdminUserID)
            {
                onboardingUser.AdminRemark = request.Remark;
                onboardingUser.AdminStatus = request.Status;
                onboardingUser.AdminVerify = true;
            }
            if (onboardingUser.supportUserVerify != true && onboardingUser.AdminVerify == true)
            {
                return Message.OnboardingMessages.SupportUserVerify;
            }

            if (UserID == Company.AdminUserID && request.Status.EqualsIgnoreCase(StatusType.Approve))
            {

                Project.Requests.User.RegistrationRequest registrationRequest = new Requests.User.RegistrationRequest();
                
                registrationRequest.Address = onboardingUser.Residential_Address;
                registrationRequest.ParentID = new  UserDAL().SelectUserIDByAliasNames(onboardingUser.Referral_Code);
                registrationRequest.AliasName = new UserTypeBal().SelectDefaultAliasNameWithUserType((int)UserType.Retailer);
                registrationRequest.FirmName = onboardingUser.FirmName;
                registrationRequest.PersonName = onboardingUser.Name;
                registrationRequest.MobileNumber = onboardingUser.MobileNumber;
                registrationRequest.EmailID = onboardingUser.EmailID;
                registrationRequest.GSTNo = onboardingUser.GSTNo;
                registrationRequest.PANNo = onboardingUser.PanNo;
                registrationRequest.AadharNo = onboardingUser.AadhaarNo;
                registrationRequest.UserTypeID = (int)UserType.Retailer;
                registrationRequest.StateID = 1;
                registrationRequest.ParentID = UserID;
                registrationRequest.JoiningByUserID = UserID;
                registrationRequest.Pincode = onboardingUser.PinCode;
                registrationRequest.WhatsAppNumber = onboardingUser.MobileNumber;
                registrationRequest.ShopAddress = onboardingUser.Business_Address;
                registrationRequest.SlabID = onboardingUser.SlabID;

                failureMessage = await new UserBal().CheckckingDuplicateAliasName(registrationRequest.AliasName);
                if (!failureMessage.EqualsIgnoreCase(StaticModel.StatusType.Ok))
                    registrationRequest.AliasName = new UserTypeBal().SelectDefaultAliasNameWithUserType((int)UserType.Retailer);

                using (ConcurrencyControl.AcquireLock(UserID, registrationRequest.AliasName))
                {
                    failureMessage = await new UserBal().Insert(registrationRequest, "0");
                    if (!failureMessage.EqualsIgnoreCase(StaticModel.StatusType.Ok))
                        return failureMessage;
                }

            }

            int Result = new OnboardingUserDal().Update(onboardingUser);
            if (Result < 0)
                return Message.OnboardingMessages.InsertFailure;

            return StaticModel.StatusType.Ok; ;
        }

        internal Project.Response.Onboarding.OnboardingWrapper SelectReport(Project.Requests.Onboarding.SelectRequest request)
        {
            isExcelOrPDF(request);
            Project.Response.Onboarding.OnboardingWrapper wrapper = new Response.Onboarding.OnboardingWrapper();
            List<Project.Response.Onboarding.OnboardingViewModel> list = new List<Project.Response.Onboarding.OnboardingViewModel>();
            try
            {
                list = new OnboardingUserDal().SelectReport(request);
            }
            catch(Exception ex) { }
            if (list == null || list.Count <= 0)
            {
                wrapper.list = list;
                wrapper.Message = Message.OnboardingMessages.DataNotFound;
                return wrapper;

            }
            if (!request.CriteriaText.EqualsIgnoreCase(""))
            {
                if (request.Criteria.EqualsIgnoreCase("CreatedDate"))
                {
                    list = list.Where(P => P.DOC.Contains(request.CriteriaText)).ToList();
                }
                if (request.Criteria.EqualsIgnoreCase("MobileNumber"))
                {
                    list = list.Where(P => P.MobileNumber.Contains(request.CriteriaText)).ToList();
                }
                if (request.Criteria.EqualsIgnoreCase("PANNo"))
                {
                    list = list.Where(P => P.PanNo.Contains(request.CriteriaText)).ToList();
                }
                if (request.Criteria.EqualsIgnoreCase("EntityName"))
                {
                    list = list.Where(P => P.FullName.ToLower().Contains(request.CriteriaText.ToLower())).ToList();
                }

            }
            list = list.AsQueryable().WhereDynamic(request.Search).ToList();
            wrapper.Count = list.Count;

            if (!request.IsExcelDownload)
            {
                list = list.AsQueryable().SkipAndTakeCount(request.SkipCount, request.TakeCount).ToList();
            }
            if (Util.IsDownload(request.IsExcelDownload, request.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }
            wrapper.list = list;
            wrapper.IsExcelDownload = request.IsExcelDownload;

            wrapper.Message = StaticModel.StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Project.Requests.Onboarding.SelectRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }
            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        internal (Project.Response.Onboarding.OnboardingViewModel, string) SelectByID(Project.Requests.Onboarding.SelectByIDRequest request)
        {
            string failureMessage = string.Empty;

            Project.Response.Onboarding.OnboardingViewModel response = new Response.Onboarding.OnboardingViewModel();
            response = new OnboardingUserDal().SelectByID(request);

            if (response == null)
            {
                failureMessage = Message.OnboardingMessages.DataNotFound;
                return (response, failureMessage);

            }

            response.list = new OnboardingUserDocumentDal().SelectByID(request.OnboardingUserID);
            failureMessage = StaticModel.StatusType.Ok;
            return (response, failureMessage);
        }

        internal string ManualInsert(Project.Requests.Onboarding.ManualInsertRequest request)
        {
            string failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            OnboardingUser onboardingUser = new OnboardingUser();
            onboardingUser.OrderID = request.OrderID;
            onboardingUser.AadhaarNo = request.AadhaarNo;
            onboardingUser.ReferenceID = request.ReferenceID;
            onboardingUser.Gender = request.Gender;
            onboardingUser.DOB = request.DOB;
            onboardingUser.PanNo = request.PanNo;
            onboardingUser.Residential_Address = request.Address;
            onboardingUser.Name = request.FullName;
            onboardingUser.GSTNo = request.GstNo;
            onboardingUser.FirmName = request.FirmName;
            onboardingUser.PinCode = request.PinCode;
            onboardingUser.Country = request.Country;
            onboardingUser.EmailID = request.EmailID;
            onboardingUser.MobileNumber = request.MobileNumber;
            onboardingUser.AccountNumber = request.AccountNumber;
            onboardingUser.IFSC = request.IFSC;
            onboardingUser.BankName = request.BankName;
            onboardingUser.Business_Address = request.Business_Address;
            onboardingUser.Residential_Address = request.Residential_Address;
            onboardingUser.Residential_Locality = request.Residential_Locality;
            onboardingUser.Residential_Locality = request.Residential_Locality;
            onboardingUser.Business_Locality = request.Business_Locality;
            onboardingUser.Country = request.Country;
            onboardingUser.State = request.State;
            onboardingUser.SupportStatus = StaticModel.StatusType.Process;
            onboardingUser.AdminStatus = StaticModel.StatusType.Process;
            onboardingUser.supportUserVerify = false;
            onboardingUser.AdminVerify = false;
            int Result = 0;
            try
            {
                Result = new OnboardingUserDal().Insert(onboardingUser);
            }
            catch (Exception e)
            {
                new Logger().Write(e);
            }

            List<Model.OnboardingUserDocument> imagesRequest = new List<OnboardingUserDocument>();

            var imageProperties = new Dictionary<string, CustomFile>{
            {"Selfie", request.SelfieImage},
            {"Pan", request.PANImage},
            {"Aadhaar", request.AadhaarImage},
            {"GST", request.GSTImage},
            {"Establishment1", request.Establishment1Image},
            {"Establishment2", request.Establishment2Image},
            {"Shop_images1", request.Shop_images1Image},
            {"Shop_images2", request.Shop_images2Image}
            };
            foreach (var imagePair in imageProperties)
            {
                if (imagePair.Value != null && !string.IsNullOrEmpty(imagePair.Value.Name))
                {
                    string path = new MediaProcessing().SaveImagesIdfy(imagePair.Value, MediaType.IdfyOnBoarding, imagePair.Key, onboardingUser.FirmName);

                    Model.OnboardingUserDocument userDocument = new Model.OnboardingUserDocument
                    {
                        Ref_ID = imagePair.Key,
                        NewImage = path
                    };

                    imagesRequest.Add(userDocument);
                }
            }
            failureMessage = OnboardingUserDocument(Result, imagesRequest);
            if (!failureMessage.EqualsIgnoreCase(StaticModel.StatusType.Ok))
                return Message.OnboardingMessages.InsertFailure;
            return failureMessage;
        }
    }
}
