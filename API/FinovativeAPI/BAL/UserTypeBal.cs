using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.UserType;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class UserTypeBal
    {
        internal List<UserTypeViewModel> SelectForSaleTarget(int UserID)
        {
            List<UserTypeViewModel> userTypes = new List<UserTypeViewModel>();
            List<UserTypeMaster> ReturnList;

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserTypeID == (int)UserType.APIUser)
            {
                return userTypes;
            }

            try
            {
                ReturnList = new UserTypeDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return userTypes;
            }

            if (ReturnList == null)
            {
                return userTypes;
            }

            //remove admin role
            userTypes = (from item in ReturnList
                         where item.UserTypeID != (int)UserType.Admin
                          && item.UserTypeID != (int)UserType.Client
                          && item.UserTypeID != (int)UserType.Accountant
                          && item.UserTypeID != (int)UserType.Developer
                          && item.UserTypeID != (int)UserType.DFOS
                          && item.UserTypeID != (int)UserType.SDFOS
                          && item.UserTypeID != (int)UserType.Support

                         select new UserTypeViewModel
                         {
                             UserTypeID = item.UserTypeID,
                             UserType = item.UserType,
                             AliasName = item.AliasName,
                         }).ToList();

            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Distributer || item.UserTypeID == (int)UserType.Retailer
                             || item.UserTypeID == (int)UserType.Client
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                             }).ToList();
            }

            if (UserTypeID == (int)UserType.Distributer)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Retailer
                              || item.UserTypeID == (int)UserType.Client
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                             }).ToList();
            }
            if (UserTypeID == (int)UserType.Retailer)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Client
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                             }).ToList();
            }

            if (UserTypeID == (int)UserType.SDFOS)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Retailer
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                             }).ToList();
            }
            if (UserTypeID == (int)UserType.DFOS)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Retailer
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                             }).ToList();
            }

            return userTypes;
        }

        internal List<UserTypeViewModel> SelectV1(int UserID)
        {
            List<UserTypeViewModel> userTypes = new List<UserTypeViewModel>();
            List<UserTypeMaster> ReturnList;

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserTypeID == (int)UserType.APIUser)
            {
                return userTypes;
            }

            try
            {
                ReturnList = new UserTypeDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return userTypes;
            }

            if (ReturnList == null)
            {
                return userTypes;
            }

            //remove admin role
            userTypes = (from item in ReturnList
                         where item.UserTypeID != (int)UserType.Admin
                         select new UserTypeViewModel
                         {
                             UserTypeID = item.UserTypeID,
                             UserType = item.UserType,
                             AliasName = item.AliasName,
                             MinimumAmount = item.FundTransferMinimumAmount,
                         }).ToList();

            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                userTypes = (from item in userTypes
                             where (item.UserTypeID == (int)UserType.Distributer || item.UserTypeID == (int)UserType.Retailer
                             || item.UserTypeID == (int)UserType.SDFOS)
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                                 MinimumAmount = item.MinimumAmount,
                             }).ToList();
            }

            if (UserTypeID == (int)UserType.Distributer)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Retailer

                              || item.UserTypeID == (int)UserType.DFOS
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                                 MinimumAmount = item.MinimumAmount,
                             }).ToList();
            }
            if (UserTypeID == (int)UserType.Retailer)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Client
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                                 MinimumAmount = item.MinimumAmount,
                             }).ToList();
            }

            if (UserTypeID == (int)UserType.SDFOS)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Distributer || item.UserTypeID == (int)UserType.Retailer
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                                 MinimumAmount = item.MinimumAmount,
                             }).ToList();
            }
            if (UserTypeID == (int)UserType.DFOS)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Retailer
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                                 MinimumAmount = item.MinimumAmount,
                             }).ToList();
            }

            return userTypes;
        }

        internal List<UserTypeViewModel> Select(int UserID)
        {
            List<UserTypeViewModel> userTypes = new List<UserTypeViewModel>();
            List<UserTypeMaster> ReturnList;

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserTypeID == (int)UserType.APIUser)
            {
                return userTypes;
            }

            try
            {
                ReturnList = new UserTypeDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return userTypes;
            }

            if (ReturnList == null)
            {
                return userTypes;
            }

            //remove admin role
            userTypes = (from item in ReturnList
                         where item.UserTypeID != (int)UserType.Admin
                         select new UserTypeViewModel
                         {
                             UserTypeID = item.UserTypeID,
                             UserType = item.UserType,
                             AliasName = item.AliasName,
                             MinimumAmount = item.FundTransferMinimumAmount,
                         }).ToList();

            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                userTypes = (from item in userTypes
                             where (item.UserTypeID == (int)UserType.Distributer || item.UserTypeID == (int)UserType.Retailer
                             || item.UserTypeID == (int)UserType.Client || item.UserTypeID == (int)UserType.SDFOS)
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                                 MinimumAmount = item.MinimumAmount,
                             }).ToList();
            }

            if (UserTypeID == (int)UserType.Distributer)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Retailer
                              || item.UserTypeID == (int)UserType.Client
                              || item.UserTypeID == (int)UserType.DFOS
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                                 MinimumAmount = item.MinimumAmount,
                             }).ToList();
            }
            if (UserTypeID == (int)UserType.Retailer)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Client
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                                 MinimumAmount = item.MinimumAmount,
                             }).ToList();
            }

            if (UserTypeID == (int)UserType.SDFOS)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Distributer || item.UserTypeID == (int)UserType.Retailer
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                                 MinimumAmount = item.MinimumAmount,
                             }).ToList();
            }
            if (UserTypeID == (int)UserType.DFOS)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Retailer
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                                 AliasName = item.AliasName,
                                 MinimumAmount = item.MinimumAmount,
                             }).ToList();
            }

            return userTypes;
        }

        internal List<UserTypeViewModel> SelectWithoutAPIUserType(int UserID)
        {
            List<UserTypeViewModel> userTypes = new List<UserTypeViewModel>();
            List<UserTypeMaster> ReturnList;

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                return userTypes;
            }

            try
            {
                ReturnList = new UserTypeDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return userTypes;
            }

            if (ReturnList == null)
            {
                return userTypes;
            }

            //remove admin role
            userTypes = (from item in ReturnList
                         where item.UserTypeID != (int)UserType.Admin
                         && item.UserTypeID != (int)UserType.APIUser
                         select new UserTypeViewModel
                         {
                             UserTypeID = item.UserTypeID,
                             UserType = item.UserType,
                         }).ToList();

            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Distributer || item.UserTypeID == (int)UserType.Retailer
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                             }).ToList();
            }

            if (UserTypeID == (int)UserType.Distributer)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Retailer
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                             }).ToList();
            }

            return userTypes;
        }

        internal List<UserTypeViewModel> SelectOnlyDistAndSuperDist(int userID)
        {
            List<UserTypeViewModel> userTypes = new List<UserTypeViewModel>();
            List<UserTypeMaster> ReturnList;
            try
            {
                ReturnList = new UserTypeDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return userTypes;
            }

            if (ReturnList == null)
            {
                return userTypes;
            }
            userTypes = (from item in ReturnList
                         where item.UserTypeID == (int)UserType.SuperDistributer
                         || item.UserTypeID == (int)UserType.Distributer
                         select new UserTypeViewModel
                         {
                             UserTypeID = item.UserTypeID,
                             UserType = item.UserType,
                         }).ToList();

            return userTypes;
        }

        internal List<UserTypeViewModel> SelectOnlyAdminDistAndSuperDist(int userID)
        {
            List<UserTypeViewModel> userTypes = new List<UserTypeViewModel>();
            List<UserTypeMaster> ReturnList;
            try
            {
                ReturnList = new UserTypeDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return userTypes;
            }

            if (ReturnList == null)
            {
                return userTypes;
            }
            userTypes = (from item in ReturnList
                         where item.UserTypeID == (int)UserType.Admin
                         || item.UserTypeID == (int)UserType.SuperDistributer
                         || item.UserTypeID == (int)UserType.Distributer
                         select new UserTypeViewModel
                         {
                             UserTypeID = item.UserTypeID,
                             UserType = item.UserType,
                         }).ToList();

            return userTypes;
        }

        internal string UpdateAliasName(UpdateAliasNameRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserTypeMaster ObjModel = null;
            try
            {
                ObjModel = new UserTypeDAL().SelectByID(updateRequest.UserTypeID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }
            ObjModel.AliasName = updateRequest.AliasName;
            ObjModel.FundTransferMinimumAmount = updateRequest.MinimumAmount;
            try
            {
                int rowsAffected = new UserTypeDAL().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return MessageSettingMessage.updateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal List<UserTypeViewModel> SelectOnlyRetailerAndAPI(int UserID)
        {
            List<UserTypeViewModel> userTypes = new List<UserTypeViewModel>();
            List<UserTypeMaster> ReturnList;
            try
            {
                ReturnList = new UserTypeDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return userTypes;
            }

            if (ReturnList == null)
            {
                return userTypes;
            }
            userTypes = (from item in ReturnList
                         where item.UserTypeID == (int)UserType.Retailer
                         || item.UserTypeID == (int)UserType.APIUser
                         select new UserTypeViewModel
                         {
                             UserTypeID = item.UserTypeID,
                             UserType = item.UserType,
                         }).ToList();

            return userTypes;
        }

        internal List<UserTypeViewModel> SelectRoleForOutside()
        {
            List<UserTypeViewModel> userTypes = new List<UserTypeViewModel>();
            List<UserTypeMaster> ReturnList;
            try
            {
                ReturnList = new UserTypeDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return userTypes;
            }

            if (ReturnList == null)
            {
                return userTypes;
            }
            userTypes = (from item in ReturnList
                         where item.UserTypeID != (int)UserType.Support
                         && item.IsOutside == true
                         select new UserTypeViewModel
                         {
                             UserTypeID = item.UserTypeID,
                             UserType = item.UserType,
                         }).ToList();

            return userTypes;
        }

        internal List<UserTypeViewModel> SelectRoleForSignUp(Requests.User.RoleForSignUpRequest Request)
        {
            List<UserTypeViewModel> userTypes = new List<UserTypeViewModel>();
            List<UserTypeMaster> ReturnList;
            try
            {
                ReturnList = new UserTypeDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return userTypes;
            }

            if (ReturnList == null)
            {
                return userTypes;
            }

            userTypes = (from item in ReturnList
                         where item.UserTypeID != (int)UserType.Support
                         && item.IsOutside == true
                         select new UserTypeViewModel
                         {
                             UserTypeID = item.UserTypeID,
                             UserType = item.UserType,
                         }).ToList();

            if (Request.ReferralCode.EqualsIgnoreCase(""))
                return userTypes;

            UserMaster referralparenttype = null;
            referralparenttype = new UserDAL().SelectByReferralCode(Request.ReferralCode);
            if (referralparenttype == null) return userTypes;

            if (referralparenttype.UserTypeID == (int)UserType.Client)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Client
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                             }).ToList();

                return userTypes;
            }

            if (referralparenttype.UserTypeID == (int)UserType.Retailer || referralparenttype.UserTypeID == (int)UserType.APIUser)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Retailer
                             || item.UserTypeID == (int)UserType.APIUser
                             || item.UserTypeID == (int)UserType.Client

                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                             }).ToList();

                return userTypes;
            }

            if (referralparenttype.UserTypeID == (int)UserType.Distributer)
            {
                userTypes = (from item in userTypes
                             where (item.UserTypeID == (int)UserType.Retailer ||
                             item.UserTypeID == (int)UserType.DFOS ||
                             item.UserTypeID == (int)UserType.APIUser ||
                             item.UserTypeID == (int)UserType.Client
                             )

                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                             }).ToList();

                return userTypes;
            }

            if (referralparenttype.UserTypeID == (int)UserType.SuperDistributer)
            {
                userTypes = (from item in userTypes
                             where (item.UserTypeID == (int)UserType.Retailer ||
                             item.UserTypeID == (int)UserType.Distributer ||
                             item.UserTypeID == (int)UserType.SDFOS ||
                             item.UserTypeID == (int)UserType.APIUser ||
                             item.UserTypeID == (int)UserType.Client
                             )

                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                             }).ToList();

                return userTypes;
            }

            return userTypes;
        }

        internal List<UserTypeViewModel> SelectOusideRoleSetting(int userID)
        {
            List<UserTypeViewModel> userTypes = new List<UserTypeViewModel>();
            List<UserTypeMaster> ReturnList;
            try
            {
                ReturnList = new UserTypeDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return userTypes;
            }

            if (ReturnList == null)
            {
                return userTypes;
            }
            userTypes = (from item in ReturnList
                         where item.UserTypeID != (int)UserType.Admin
                         && item.UserTypeID != (int)UserType.Support
                         && item.UserTypeID != (int)UserType.Accountant
                           && item.UserTypeID != (int)UserType.Distributer
                             && item.UserTypeID != (int)UserType.SuperDistributer
                             && item.UserTypeID != (int)UserType.SDFOS
                                 && item.UserTypeID != (int)UserType.DFOS
                         select new UserTypeViewModel
                         {
                             UserTypeID = item.UserTypeID,
                             UserType = item.UserType,
                             IsOutside = item.IsOutside,
                         }).ToList();
            return userTypes;
        }

        internal List<UserTypeViewModel> SelectRoleWithoutSupport(int UserID)
        {
            List<UserTypeViewModel> userTypes = new List<UserTypeViewModel>();
            List<UserTypeMaster> ReturnList;
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser || UserTypeID == (int)UserType.Client)
            {
                return userTypes;
            }
            try
            {
                ReturnList = new UserTypeDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return userTypes;
            }

            if (ReturnList == null)
            {
                return userTypes;
            }

            userTypes = (from item in ReturnList
                         where item.UserTypeID != (int)UserType.Admin
                         && item.UserTypeID != (int)UserType.Support
                         && item.UserTypeID != (int)UserType.Accountant
                         && item.UserTypeID != (int)UserType.SDFOS
                         && item.UserTypeID != (int)UserType.DFOS
                         select new UserTypeViewModel
                         {
                             UserTypeID = item.UserTypeID,
                             UserType = item.UserType,
                         }).ToList();

            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Distributer || item.UserTypeID == (int)UserType.Retailer
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                             }).ToList();
            }

            if (UserTypeID == (int)UserType.Distributer)
            {
                userTypes = (from item in userTypes
                             where item.UserTypeID == (int)UserType.Retailer
                             select new UserTypeViewModel
                             {
                                 UserTypeID = item.UserTypeID,
                                 UserType = item.UserType,
                             }).ToList();
            }

            return userTypes;
        }

        internal string IsOutsideActivate(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserTypeMaster ObjModel = null;
            try
            {
                ObjModel = new UserTypeDAL().SelectByID(updateRequest.UserTypeID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }
            ObjModel.IsOutside = updateRequest.IsOutside;

            try
            {
                int rowsAffected = new UserTypeDAL().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return MessageSettingMessage.updateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string SelectDefaultAliasName(int UserTypeID)
        {
            string AliasName = "";
            int LastUserID = 0;
            List<UserTypeMaster> ReturnList = new UserTypeDAL().Select();
            try
            {
                LastUserID = new UserDAL().SelectLastUserID();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            AliasName = ReturnList.FirstOrDefault(x => x.UserTypeID == UserTypeID).AliasName;

            return $"{AliasName}{LastUserID}";
        }



        internal string SelectDefaultAliasNameWithUserType(int UserTypeID)
        {
            string AliasName = "";
            int LastUserID = 0;
            List<UserTypeMaster> ReturnList = new UserTypeDAL().Select();
            try
            {
                LastUserID = new UserDAL().SelectLastUserID(UserTypeID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            
            AliasName = ReturnList.FirstOrDefault(x => x.UserTypeID == UserTypeID).AliasName;

            return $"{AliasName}{LastUserID.ToString("D6")}";
        }

        internal decimal SelectMinimumAmount(int UserTypeID)
        {
            decimal MinimumAmount = 1M;
            List<UserTypeMaster> ReturnList = new UserTypeDAL().Select();

            MinimumAmount = ReturnList.FirstOrDefault(x => x.UserTypeID == UserTypeID).FundTransferMinimumAmount;

            return MinimumAmount;
        }
    }
}