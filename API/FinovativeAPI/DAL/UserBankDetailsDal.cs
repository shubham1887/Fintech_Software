using Project.Model;
using Project.Processing;
using Project.Response.UserBankDetails;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Project.DAL
{
    public class UserBankDetailsDal
    {
        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UserBankDetails>>();
            cache.Remove(CacheKeys.UserBankDetails);
        }

        internal List<UserBankDetails> Select()
        {
            var _bankDetails = new MemoryCacheProcessing<List<UserBankDetails>>();
            List<UserBankDetails> details = _bankDetails.GetOrCreate(CacheKeys.UserBankDetails, () => SelectWithoutCache());
            return details;
        }

        private List<UserBankDetails> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.UserBankDetails.ToList();
        }

        internal List<UserBankDetails> SelectByUserID(int userID)
        {
            return Select().Where(b => b.UserID == userID && b.IsDelete == false).ToList();
        }

        internal int Insert(UserBankDetails objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserBankDetails.Add(objModel);
            context.SaveChanges();
            return objModel.UserBankDetailsID;
        }

        internal UserBankDetails SelectByID(int UserBankDetailsID)
        {
            using var context = new DatabaseContext();
            return context.UserBankDetails.Find(UserBankDetailsID);
        }

        internal int Update(UserBankDetails objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserBankDetails.Update(objModel);
            return context.SaveChanges();
        }

        internal List<ViewModel> SelectParentBanks(int ParentID)
        {
            List<ViewModel> viewModels = new List<ViewModel>();
            List<UserMaster> userList = new UserDAL().Select().Where(u => u.UserID == ParentID).ToList();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            List<UserBankDetails> list = Select();
            viewModels = (from bank in list
                          join user in userList on bank.UserID equals user.UserID
                          join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                          where (bank.UserID == ParentID || bank.UserID == Company.AdminUserID)
                          && bank.IsDelete == false
                          select new ViewModel
                          {
                              Role = usertype.UserType,
                              FirmName = user.FirmName,
                              UserBankDetailsID = bank.UserBankDetailsID,
                              HolderName = bank.HolderName,
                              Branch = bank.Branch,
                              UserID = bank.UserID,
                              BankName = bank.BankName,
                              AccountNumber = bank.AccountNumber,
                              UPINumber = bank.UPINumber,
                              IFSC = bank.IFSC,
                              QRCodeImageURL = String.Concat(Company.WebAPIURL, bank.QRCodeImageURL),                             
                          }).ToList();

            return viewModels;
        }

        internal List<ViewModel> SelectCompanyBanks()
        {
            List<UserMaster> userList = new UserDAL().Select().Where(u => u.UserID == Company.AdminUserID).ToList();
            List<UserBankDetails> list = Select().Where(u => u.IsDelete == false).ToList();
            return (from bank in list
                    join user in userList on bank.UserID equals user.UserID
                    where bank.UserID == Company.AdminUserID
                    select new ViewModel
                    {
                        Role = Company.ClientName,
                        FirmName = user.FirmName,
                        UserBankDetailsID = bank.UserBankDetailsID,
                        HolderName = bank.HolderName,
                        Branch = bank.Branch,
                        UserID = bank.UserID,
                        BankName = bank.BankName,
                        AccountNumber = bank.AccountNumber,
                        UPINumber = bank.UPINumber,
                        IFSC = bank.IFSC,
                        QRCodeImageURL = String.Concat(Company.WebAPIURL, bank.QRCodeImageURL),
                    }).ToList();
        }

        internal UserBankDetails SelectBankByUserID(int userID)
        {
            return Select().Where(b => b.UserID == userID && b.IsDelete == false && b.IsOn == true).FirstOrDefault();
        }
        internal List<ViewModel> SelectBankForDropDown(int ParentID)
        {
            List<ViewModel> viewModels = new List<ViewModel>();
            List<UserMaster> userList = new UserDAL().Select().Where(u => u.UserID == ParentID).ToList();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
            List<UserBankDetails> list = Select();
            viewModels = (from bank in list
                          join user in userList on bank.UserID equals user.UserID
                          join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                          where bank.UserID == ParentID 
                          && bank.IsDelete == false
                          select new ViewModel
                          {
                              UserBankDetailsID = bank.UserBankDetailsID,
                              BankName = bank.BankName.ConcatBankNameBankAccountNumberIfscCode(bank.AccountNumber,bank.IFSC),
                          }).ToList();

            return viewModels;
        }
    }
}