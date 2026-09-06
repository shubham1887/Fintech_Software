using Processing.Processing;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.UserBankDetails;
using Project.Response.User;
using Project.Response.UserBankDetails;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UserBankDetailsBal
    {
        internal List<ViewModel> Select(int userID)
        {
            List<ViewModel> list = new List<ViewModel>();
            List<UserBankDetails> dbList = new List<UserBankDetails>();

            try
            {
                dbList = new UserBankDetailsDal().SelectByUserID(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            foreach (UserBankDetails bank in dbList)
            {
                list.Add(new ViewModel
                {
                    UserBankDetailsID = bank.UserBankDetailsID,
                    HolderName = bank.HolderName,
                    Branch = bank.Branch,
                    UserID = bank.UserID,
                    BankName = bank.BankName,
                    AccountNumber = bank.AccountNumber,
                    UPINumber = bank.UPINumber,
                    IFSC = bank.IFSC,
                    QRCodeImageURL = Company.WebAPIURL + bank.QRCodeImageURL
                });
            }
            return list;
        }

        internal string Insert(InsertRequest insertRequest, int UserID, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (UserID != (Company.AdminUserID))
            {
                string BankDetailEntryForUpline = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.BankDetailEntryForUpline), "Yes");

                if (BankDetailEntryForUpline.EqualsIgnoreCase("No"))
                {
                    failureMessage = UserBankDetailsMessage.AccessDenied;
                    return failureMessage;
                }
            }
            if (UserID != ((int)UserType.SuperDistributer))
            {
                string BankDetailEntryForUpline = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.BankDetailEntryForUpline), "Yes");

                if (BankDetailEntryForUpline.EqualsIgnoreCase("No"))
                {
                    failureMessage = UserBankDetailsMessage.AccessDenied;
                    return failureMessage;
                }
            }
            if (insertRequest.Document.Size.Length > 0 && insertRequest.Document.Type.Length > 0)
                failureMessage = insertRequest.IsValidModalStateForDocument();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            string path = "";
            try
            {
                path = new MediaProcessing().SaveImage(insertRequest.Document, MediaType.QRCode, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            UserBankDetails ObjModel = new UserBankDetails
            {
                UserID = UserID,
                AccountNumber = insertRequest.AccountNumber,
                Branch = insertRequest.Branch,
                BankName = insertRequest.BankName,
                IFSC = insertRequest.IFSC,
                HolderName = insertRequest.HolderName,
                UPINumber = insertRequest.UPINumber ?? "",
                QRCodeImageURL = path,
                IsDelete = false
            };

            UserBankDetails model = null;
            try
            {
                model = new UserBankDetailsDal().SelectByID(insertRequest.UserBankDetailsID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            int rowsAffected;
            if (model == null)
                try
                {
                    response = UserBankDetailsMessage.InsertFail;
                    rowsAffected = new UserBankDetailsDal().Insert(ObjModel);
                    if (rowsAffected > 0)
                        response = UserBankDetailsMessage.InsertSuccess;
                    else
                        failureMessage = response;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
            else
            {
                response = UserBankDetailsMessage.UpdateFail;
                model.AccountNumber = insertRequest.AccountNumber;
                model.BankName = insertRequest.BankName;
                model.Branch = insertRequest.Branch;
                model.IFSC = insertRequest.IFSC;
                model.HolderName = insertRequest.HolderName;
                model.UPINumber = insertRequest.UPINumber ?? "";
                model.QRCodeImageURL = path ?? model.QRCodeImageURL;
                try
                {
                    rowsAffected = new UserBankDetailsDal().Update(model);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return response;
                }
                if (rowsAffected > 0)

                    response = UserBankDetailsMessage.UpdateSuccess;
                else
                    failureMessage = response;
            }
            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Delete(SelectRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserBankDetails ObjModel = null;
            try
            {
                ObjModel = new UserBankDetailsDal().SelectByID(deleteRequest.UserBankDetailsID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ObjModel == null)
                return UserBankDetailsMessage.NotFound;

            ObjModel.IsDelete = true;
            ObjModel.UpdatedDOC = DateTime.Now;

            try
            {
                int rowsAffected = new UserBankDetailsDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return UserBankDetailsMessage.DeleteFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal List<ViewModel> SelectByParentID(SelectByParentID selectRequest)
        {
            List<ViewModel> list = new List<ViewModel>();
            List<UserBankDetails> dbList = new List<UserBankDetails>();

            try
            {
                dbList = new UserBankDetailsDal().SelectByUserID(selectRequest.ParentID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            foreach (UserBankDetails bank in dbList)
            {
                list.Add(new ViewModel
                {
                    UserBankDetailsID = bank.UserBankDetailsID,
                    HolderName = bank.HolderName,
                    Branch = bank.Branch,
                    UserID = bank.UserID,
                    BankName = bank.BankName,
                    AccountNumber = bank.AccountNumber,
                    UPINumber = bank.UPINumber,
                    IFSC = bank.IFSC
                });
            }
            return list;
        }

        internal List<ViewModel> SelectParentBanks(int UserID)
        {
            List<ViewModel> list = new List<ViewModel>();
            ParentViewModel parent = new UserDAL().SelectParent(UserID);
            try
            {
                list = new UserBankDetailsDal().SelectParentBanks(parent.ParentID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        internal List<ViewModel> SelectCompanyBanks(int UserID)
        {
            List<ViewModel> list = new List<ViewModel>();
            ParentViewModel parent = new UserDAL().SelectParent(UserID);

            if (parent.ParentID == Company.AdminUserID)
                return list;
            try
            {
                list = new UserBankDetailsDal().SelectCompanyBanks();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }
        internal List<ViewModel> SelectBankForDropDown(int ParentID)
        {
            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new UserBankDetailsDal().SelectBankForDropDown(ParentID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }
    }
}