using Project.Model;
using Project.Processing;
using Project.Response.Onboarding;
using System.Collections.Generic;
using System.Linq;
namespace Project.DAL
{
    public class OnboardingUserDal
    {
        private readonly string Key = "_OnboardingUser";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<Project.Response.Onboarding.OnboardingViewModel>>();
            cache.Remove(Key);
        }
        public int Insert(OnboardingUser ObjModel)
        {
            removeCache();
            using var context = new DatabaseContext();

            context.OnboardingUser.Add(ObjModel);
            context.SaveChanges();

            return ObjModel.OnboardingUserID;
        }
        public int Update(OnboardingUser ObjModel)
        {
            removeCache();
            using var context = new DatabaseContext();
            context.OnboardingUser.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.OnboardingUserID;
        }
        internal List<Project.Response.Onboarding.OnboardingViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<Project.Response.Onboarding.OnboardingViewModel>>();
            List<Project.Response.Onboarding.OnboardingViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }
        public OnboardingUser SelectByID(int OnboardingUserID)
        {
            var context = new DatabaseContext();
            return context.OnboardingUser.Find(OnboardingUserID);
        }
        public OnboardingUser Duplicate(string ID, string ProfileID, string PanNo)
        {
            var context = new DatabaseContext();
            return context.OnboardingUser.Where(txn => txn.ID == ID && txn.ProfileID == ProfileID && txn.PanNo == PanNo).FirstOrDefault();

        }
        internal List<Project.Response.Onboarding.OnboardingViewModel> SelectByIDWithCache(int OnboardingUserID)
        {
            List<Project.Response.Onboarding.OnboardingViewModel> list = Select();
            return list.Where(user => user.OnboardingUserID == OnboardingUserID).ToList();
        }

        internal List<Project.Response.Onboarding.OnboardingViewModel> SelectReport(Project.Requests.Onboarding.SelectRequest request)
        {
            
            List<UserMaster> UserMaster = new UserDAL().Select();
            List<Project.Response.Onboarding.OnboardingViewModel> list = Select();
            List<UserTypeMaster> userTypeMaster = new UserTypeDAL().Select();
            list = (from user in list
                    join u in UserMaster
                     on user.SupportUserID equals u.UserID into gj
                    from SupportUser in gj.DefaultIfEmpty()
                    join userTypemaster in userTypeMaster
                    on user.CreateRoleID equals userTypemaster.UserTypeID into Type
                    from userType in Type.DefaultIfEmpty()
                    where (!request.CheckerStatus.EqualsIgnoreCase("all") ? user.AdminStatus == request.CheckerStatus : 2 > 1)
                     && (!request.MakerStatus.EqualsIgnoreCase("all") ? user.SupportStatus == request.MakerStatus : 2 > 1)
                    select new OnboardingViewModel
                    {
                        OnboardingUserID = user.OnboardingUserID,
                        DOC = user.DOC,
                        ID = user.ID,
                        ProfileID = user.ProfileID,
                        ReferenceID = user.ReferenceID,
                        FullName = user.FullName,
                        DOB = user.DOB,
                        PanNo = user.PanNo,
                        SlabID = user.SlabID,
                        Residential_Address = user.Residential_Address,
                        Business_Address = user.Business_Address,
                        GSTNo = user.GSTNo,
                        AadhaarNo = user.AadhaarNo,
                        PinCode = user.PinCode,
                        Country = user.Country,
                        EmailID = user.EmailID,
                        MobileNumber = user.MobileNumber,
                        FirmName = user.FirmName,
                        AccountNumber = user.AccountNumber,
                        IFSC = user.IFSC,
                        Gender = user.Gender,
                        Address_Proof = user.Address_Proof,
                        Address_Proof_Document_Number = user.Address_Proof_Document_Number,
                        Employee_Details = new Location_PinCodeMasterDal().SelectByEmployeeID(user.EmployeeID),
                        ReferralDetails = new UserDAL().SelectByRefenceID(user.Referral_Code),
                        SupportUserID = user.SupportUserID,
                        AdminRemark = user.AdminRemark,
                        SupportRemark = user.SupportRemark,
                        SupportVerify = user.SupportVerify,
                        AdminVerify = user.AdminVerify,
                        SupportStatus = user.SupportStatus,
                        AdminStatus = user.AdminStatus,
                        Residential_Locality = user.Residential_Locality,
                        Business_Locality = user.Business_Locality,
                        CreateRoleID = user.CreateRoleID,
                        EmployeeID = user.EmployeeID,
                        Referral_Code = user.Referral_Code,
                        Business_District = user.Business_District,
                        Business_Pincode = user.Business_Pincode,
                        Business_State = user.Business_State,
                        Residential_State = user.Residential_State,
                        Category = user.Category,
                        Role = userType?.UserType ?? "",
                        Handicapped = user.Handicapped,
                        Highest_Qualification = user.Highest_Qualification,
                        Occupation = user.Occupation,
                        Operating_Time = user.Operating_Time,
                        Working_Days = user.Working_Days,
                        Latitude = user.Latitude,
                        Longitude = user.Longitude,
                        GST_Address = user.GST_Address,
                        LineOfBusiness = user.LineOfBusiness,
                        Type_of_Shop = user.Type_of_Shop,
                        OrderID = user.OrderID,
                        IIBF_Certification_Number = user.IIBF_Certification_Number,
                        IIBF_Certification = user.IIBF_Certification,
                        VerifiedPage = user.VerifiedPage,
                                           }
                    ).ToList();



            return list;


        }
        internal Project.Response.Onboarding.OnboardingViewModel SelectByID(Project.Requests.Onboarding.SelectByIDRequest request)
        {
            List<Project.Response.Onboarding.OnboardingViewModel> list = Select();
            List<UserMaster> usersMaster = new UserDAL().Select();
            List<UserTypeMaster> userTypeMaster = new UserTypeDAL().Select();
            return (from user in list
                    where user.OnboardingUserID == request.OnboardingUserID

                    join userTypemaster in userTypeMaster
                      on user.CreateRoleID equals userTypemaster.UserTypeID into Type
                    from userType in Type.DefaultIfEmpty()
                    join u in usersMaster
                  on user.SupportUserID equals u.UserID into gj
                    from SupportUser in gj.DefaultIfEmpty()

                    select new Project.Response.Onboarding.OnboardingViewModel
                    {
                        OrderID = user.OrderID,
                        OnboardingUserID = user.OnboardingUserID,
                        SlabID = user.SlabID,
                        ID = user.ID,
                        ProfileID = user.ProfileID,
                        ReferenceID = user.ReferenceID,
                        FullName = user.FullName,
                        DOB = user.DOB,
                        PanNo = user.PanNo,
                        AadhaarNo = user.AadhaarNo,
                        PinCode = user.PinCode,
                        Country = user.Country,
                        EmailID = user.EmailID,
                        MobileNumber = user.MobileNumber,
                        FirmName = user.FirmName,
                        AdminStatus = user.AdminStatus,
                        Residential_Address = user.Residential_Address,
                        Business_Address = user.Business_Address,
                        GSTNo = user.GSTNo,
                        AccountNumber = user.AccountNumber,
                        BeneficiaryName = user.BeneficiaryName,
                        IFSC = user.IFSC,
                        Gender = user.Gender,
                        SupportUserID = user.SupportUserID,
                        AdminRemark = user.AdminRemark,
                        SupportRemark = user.SupportRemark,
                        SupportVerify = user.SupportVerify,
                        AdminVerify = user.AdminVerify,
                        SupportStatus = user.SupportStatus,
                        Residential_District = user.Residential_District,
                        Residential_Locality = user.Residential_Locality,
                        Business_Locality = user.Business_Locality,
                        CreateRoleID = user.CreateRoleID,
                        EmployeeID = user.EmployeeID,
                        Referral_Code = user.Referral_Code,
                        Business_District = user.Business_District,
                        Business_Pincode = user.Business_Pincode,
                        Business_State = user.Business_State,
                        Residential_State = user.Residential_State,
                        Category = user.Category,
                        Employee_Details = new Location_PinCodeMasterDal().SelectByEmployeeID(user.EmployeeID),
                        ReferralDetails = new UserDAL().SelectByRefenceID(user.Referral_Code),
                        Role = userType?.UserType ?? "",
                        Handicapped = user.Handicapped,
                        Highest_Qualification = user.Highest_Qualification,
                        Occupation = user.Occupation,
                        Operating_Time = user.Operating_Time,
                        Working_Days = user.Working_Days,
                        Latitude = user.Latitude,
                        Longitude = user.Longitude,
                        Trade_pan = user.Trade_pan,
                        Trade_name = user.Trade_name,
                        Account_exists = user.Account_exists,
                        Address_Proof = user.Address_Proof,
                        LineOfBusiness = user.LineOfBusiness,
                        Type_of_Shop = user.Type_of_Shop,
                        GST_Address = user.GST_Address,
                        IIBF_Certification_Number = user.IIBF_Certification_Number,
                        IIBF_Certification = user.IIBF_Certification,
                        VerifiedPage = user.VerifiedPage,
                        Address_Proof_Document_Number = user.Address_Proof_Document_Number,
                        SupportUserName = SupportUser?.FirmName ?? "".ConcatNameAliasName(SupportUser?.MobileNumber ?? "")
                    }
                    ).FirstOrDefault();




        }
        private List<Project.Response.Onboarding.OnboardingViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from user in context.OnboardingUser
                        orderby user.OnboardingUserID descending
                        select new Project.Response.Onboarding.OnboardingViewModel
                        {

                            OnboardingUserID = user.OnboardingUserID,
                            ID = user.ID,
                            ProfileID = user.ProfileID,
                            FullName = user.Name,
                            DOB = user.DOB,
                            PanNo = user.PanNo,
                            AadhaarNo = user.AadhaarNo,
                            Residential_Address = user.Residential_Address,
                            Business_Address = user.Business_Address,
                            GSTNo = user.GSTNo,
                            PinCode = user.PinCode,
                            Gender = user.Gender,
                            Country = user.Country,
                            EmailID = user.EmailID,
                            MobileNumber = user.MobileNumber,
                            FirmName = user.FirmName,
                            AccountNumber = user.AccountNumber,
                            IFSC = user.IFSC,
                            BeneficiaryName = user.BeneficiaryName,
                            SupportUserID = user.supportUserID,
                            AdminRemark = user.AdminRemark,
                            SupportRemark = user.SupportRemark,
                            SupportVerify = user.supportUserVerify,
                            SupportStatus = user.SupportStatus,
                            AdminStatus = user.AdminStatus,
                            AdminVerify = user.AdminVerify,
                            Residential_Locality = user.Residential_Locality,
                            Business_Locality = user.Business_Locality,
                            SlabID = user.SlabID,
                            CreateRoleID = user.CreateRole,
                            EmployeeID = user.EmployeeID,
                            Referral_Code = user.Referral_Code,
                            Business_District = user.Business_District,
                            Business_Pincode = user.Business_Pincode,
                            Business_State = user.Business_State,
                            Residential_State = user.State,
                            Residential_District = user.Residential_District,
                            Category = user.Category,
                            Handicapped = user.Handicapped,
                            Highest_Qualification = user.Highest_Qualification,
                            Occupation = user.Occupation,
                            Operating_Time = user.Operating_Time,
                            Working_Days = user.Working_Days,
                            Latitude = user.Latitude,
                            Longitude = user.Longitude,
                            Trade_pan = user.pan_number,
                            Trade_name = user.trade_name,
                            Account_exists = user.account_exists,
                            Address_Proof = user.Address_Proof,
                            Address_Proof_Document_Number = user.Address_Proof_Document_Number,
                            LineOfBusiness = user.LineOfBusiness,
                            Type_of_Shop = user.Type_of_Shop,
                            GST_Address = user.GST_Address,
                            IIBF_Certification_Number = user.IIBF_Certification_Number,
                            IIBF_Certification = user.IIBF_Certification,
                            VerifiedPage = user.VerifiedPage,
                            OrderID = user.OrderID,
                            DOC = user.DOC.LongFormatDate()
                        }).ToList();
            }
        }

    }
}
