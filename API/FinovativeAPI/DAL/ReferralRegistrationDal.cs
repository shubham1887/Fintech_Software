using Project.Model;
using Project.Processing;
using Project.Requests.ReferralRegistration;
using Project.Response.ReferralRegistration;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ReferralRegistrationDal
    {
        public readonly string key = "_ReferralRegistration";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ReferralRegistration>>();
            cache.Remove(key);
        }

        internal int Insert(ReferralRegistration objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.ReferralRegistration.Add(objModel);
                return context.SaveChanges();
            }
        }

        internal int Update(ReferralRegistration model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.ReferralRegistration.Update(model);
                return context.SaveChanges();
            }
        }

        internal List<ReferralRegistration> Select()
        {
            var _list = new MemoryCacheProcessing<List<ReferralRegistration>>();
            List<ReferralRegistration> list = _list.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        private List<ReferralRegistration> SelectWithoutCache()
        {
            List<ReferralRegistration> list;
            using (var context = new DatabaseContext())
            {
                list = (from item in context.ReferralRegistration
                        select item).ToList();
            }

            return list;
        }

        internal ReferralRegistration SelectByID(int UserID)
        {
            ReferralRegistration item = Select().FirstOrDefault(S => S.UserID == UserID);
            return item;
        }

        internal List<ReferralRegistration> SelectByCreatedID(int CreatedByUserID)
        {
            List<ReferralRegistration> list;
            using (var context = new DatabaseContext())
            {
                list = (from item in context.ReferralRegistration
                        where item.CreatedByUserID == CreatedByUserID
                        select item).ToList();
            }

            return list;
        }

        internal List<Response.User.BeneficialUserViewModel> SelectBeneficialUser()
        {
            List<BeneficialUserViewModel> mainlist = new List<BeneficialUserViewModel>();

            using var context = new DatabaseContext();
            var referallistMain = (from txn in context.TransactionMaster
                                join referral in context.ReferralRegistration on txn.UserID equals referral.UserID
                                where
                                txn.Status == StatusType.Success
                                && txn.ServiceID == (int)ServiceCode.Mobile
                                && referral.IsBenefitUtilize == false
                                select new
                                {
                                    Amount = txn.Amount,
                                    ReferralAmount = referral.ReferralAmount,
                                    UserID = txn.UserID,
                                    WalletID = txn.WalletID,
                                    ReferralUser = referral.UserMaster.FirmName,
                                    CreatedByUserID = referral.CreatedByUserID,
                                    ReferralCreatedUserID= referral.UserID
                                }).ToList();

            var referalgrouplist = (from item in referallistMain
                               group item by new { item.UserID, item.ReferralAmount, item.CreatedByUserID, item.ReferralUser, item.WalletID, item.ReferralCreatedUserID } into grp
                               select new
                               {
                                   ReferralAmount = grp.Key.ReferralAmount,
                                   WalletID = grp.Key.WalletID,
                                   UserID = grp.Key.UserID,
                                   ReferralUser = grp.Key.ReferralUser,
                                   CreatedByUserID = grp.Key.CreatedByUserID,
                                   TotalAmount = grp.Sum(tr => tr.Amount),
                                   ReferralCreatedUserID = grp.Key.ReferralCreatedUserID,
                               }).ToList();

            List<UserMaster> userlist = new UserDAL().Select();

            mainlist = (from txn in referalgrouplist
                        join user in userlist on txn.CreatedByUserID equals user.UserID
                        where
                         user.ReferralBenefitMinAmount <= txn.TotalAmount &&
                         user.ReferralAmount > 0.001M
                        select new BeneficialUserViewModel
                        {
                            WalletID = txn.WalletID,
                            UserID = txn.UserID,
                            ReferralUser = txn.ReferralUser,
                            ReceiverID = txn.CreatedByUserID,
                            Receiver = user.FirmName,
                            ReferralAmount = txn.ReferralAmount,
                            ReferralBenefitMinAmount = user.ReferralBenefitMinAmount,
                            ReferralCode = user.ReferralCode,
                            ReferralCreatedUserID= txn.ReferralCreatedUserID.ToString()
                        }).ToList();

            return mainlist;
        }

        internal List<ViewModel> SelectReport(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
            {
                list = (from Referral in context.ReferralRegistration
                        join createduser in context.UserMaster on Referral.CreatedByUserID equals createduser.UserID
                        where Referral.DOCInt >= From && Referral.DOCInt <= To

                        orderby Referral.DOC descending
                        select new ViewModel
                        {
                            ReferralRegistrationID = Referral.ReferralRegistrationID,
                            UserID = Referral.UserID,
                            CreatedByUserID = Referral.CreatedByUserID,
                            ReferralUser = Referral.UserMaster.FirmName.ConcatNameMobileAliasName(Referral.UserMaster.MobileNumber, Referral.UserMaster.AliasName),
                            ReferredUser = createduser.FirmName.ConcatNameMobileAliasName(createduser.MobileNumber, createduser.AliasName),
                            ReferralAmount = Referral.ReferralAmount,
                            IsBenefitUtilize = Referral.IsBenefitUtilize,
                            CreatedDOC = Referral.DOC.LongFormatDate(),
                            BenefitUtilizedDOC = Referral.UpdatedDOC.LongFormatDate(),
                        }).ToList();
            }

            if (UserID != Company.AdminUserID)
            {
                list = (from item in list
                        where item.CreatedByUserID == UserID
                        select item).ToList();
            }

            if (selectRequest.Status.EqualsIgnoreCase(StatusType.Success))
            {
                list = (from item in list
                        where item.IsBenefitUtilize == true
                        select item).ToList();
            }

            if (selectRequest.Status.EqualsIgnoreCase(StatusType.Process))
            {
                list = (from item in list
                        where item.IsBenefitUtilize == false
                        select item).ToList();
            }

            return list;
        }       

        internal List<ViewModel> SelectReferredUserReport(SelectReferralRequest selectRequest,int UserID)
        {          
            List<ViewModel> list = new List<ViewModel>();
            List<UserMaster> usersList = new UserDAL().Select();

            List<UserTypeMaster> usertypelist = new UserTypeDAL().Select();
      
                list = (from User in usersList
                        join usertype in usertypelist on User.UserTypeID equals usertype.UserTypeID 
                        where User.ReferralAmount > 0 || User.ReferralBenefitMinAmount > 0


                        orderby User.DOC descending
                        select new ViewModel
                        {                           
                            UserID = User.UserID,                         
                            ReferralUser = User.FirmName.ConcatNameMobileAliasName(User.MobileNumber, User.AliasName),                           
                            ReferralAmount = User.ReferralAmount,                          
                            TransactionAmount = User.ReferralBenefitMinAmount,
                            UserType = usertype.UserType,
                        }).ToList();

                if (selectRequest.UserID != 0)
                {
                    list = (from item in list
                            where item.UserID == selectRequest.UserID
                            select item).ToList();
                }           

            return list;
        }
    }
}