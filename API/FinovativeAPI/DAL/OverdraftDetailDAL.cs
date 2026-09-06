using Project.Model;
using Project.Processing;
using Project.Requests.Overdraft;
using Project.Response.Overdraft;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OverdraftDetailDAL
    {
        private readonly string key = "_OverdraftDetail";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<OverdraftDetail>>();
            cache.Remove(key);
        }

        internal int Insert(OverdraftDetail ObjModel)
        {
            RemoveCache();
            int result = -1;
            try
            {
                using (var context = new DatabaseContext())
                {
                    context.OverdraftDetail.Add(ObjModel);
                    context.SaveChanges();
                    result = ObjModel.OverdraftDetailID;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal int Update(OverdraftDetail ObjModel)
        {
            RemoveCache();
            int result = -1;
            try
            {
                using (var context = new DatabaseContext())
                {
                    context.OverdraftDetail.Update(ObjModel);
                    context.SaveChanges();
                    result = ObjModel.OverdraftDetailID;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return result;
        }

        internal List<RequestViewModel> SelectLast10Request(int UserID)
        {
            List<RequestViewModel> list = new List<RequestViewModel>();
            using (var context = new DatabaseContext())
            {
                list = (from overdraftDetail in context.OverdraftDetail
                        where
                        overdraftDetail.UserID == UserID
                        orderby overdraftDetail.DOC descending

                        select new RequestViewModel
                        {
                            OverdraftDetailID = overdraftDetail.OverdraftDetailID,
                            User = overdraftDetail.User.FirmName.ConcatNameMobileAliasName(overdraftDetail.User.MobileNumber, overdraftDetail.User.AliasName),
                            Amount = overdraftDetail.OverdraftAmount,
                            Wallet = overdraftDetail.Wallet.Name,
                            Status = overdraftDetail.Status,
                            UserRemark = overdraftDetail.UserRemark,
                            AdminRemark = overdraftDetail.AdminRemark,
                            DOC = overdraftDetail.DOC.ToString(Messages.DOCDisplayFormat),
                        }).Take(10).ToList();
            }

            return list;
        }

        internal OverdraftDetail SelectByID(int overdraftDetailID)
        {
            using var context = new DatabaseContext();
            return context.OverdraftDetail.FirstOrDefault(W => W.OverdraftDetailID == overdraftDetailID);
        }

        internal List<RequestViewModel> SelectRequestReport(RequestReport selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();
            using (var context = new DatabaseContext())
            {
                list = (from overdraftDetail in context.OverdraftDetail
                        where
                            overdraftDetail.UserID == UserID
                            && overdraftDetail.DOCInt >= From
                            && overdraftDetail.DOCInt <= To
                        orderby overdraftDetail.DOC descending

                        select new RequestViewModel
                        {
                            OverdraftDetailID = overdraftDetail.OverdraftDetailID,
                            User = overdraftDetail.User.FirmName.ConcatNameMobileAliasName(overdraftDetail.User.MobileNumber, overdraftDetail.User.AliasName),
                            Amount = overdraftDetail.OverdraftAmount,
                            Wallet = overdraftDetail.Wallet.Name,
                            Status = overdraftDetail.Status,
                            UserRemark = overdraftDetail.UserRemark,
                            AdminRemark = overdraftDetail.AdminRemark,
                            DOC = overdraftDetail.DOC.ToString(Messages.DOCDisplayFormat),
                        }).Take(10).ToList();
            }

            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            return list;
        }

        internal List<RequestViewModel> SelectRequestReportForAdmin(RequestReport selectRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<RequestViewModel> list = new List<RequestViewModel>();
            using (var context = new DatabaseContext())
            {
                list = (from overdraftDetail in context.OverdraftDetail
                        where
                           overdraftDetail.DOCInt >= From
                            && overdraftDetail.DOCInt <= To
                        orderby overdraftDetail.DOC descending

                        select new RequestViewModel
                        {
                            OverdraftDetailID = overdraftDetail.OverdraftDetailID,
                            User = overdraftDetail.User.FirmName.ConcatNameMobileAliasName(overdraftDetail.User.MobileNumber, overdraftDetail.User.AliasName),
                            Amount = overdraftDetail.OverdraftAmount,
                            Wallet = overdraftDetail.Wallet.Name,
                            Status = overdraftDetail.Status,
                            UserRemark = overdraftDetail.UserRemark,
                            AdminRemark = overdraftDetail.AdminRemark,
                            DOC = overdraftDetail.DOC.ToString(Messages.DOCDisplayFormat),
                        }).Take(10).ToList();
            }

            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            return list;
        }

        internal RequestViewModel SelectDetailByID(int userID, int overdraftDetailID)
        {
            OverdraftDetail detail = null;
            using (var context = new DatabaseContext())
            {
                detail = context.OverdraftDetail.FirstOrDefault(o => o.OverdraftDetailID == overdraftDetailID);
            }

            decimal SenderBalance = new UserWalletMasterDal().SelectSumByUserIDAndWalletID(userID, detail.WalletID);

            decimal OverdraftBalance = new OverdraftMasterDAL().SelectSumByUserIDAndWalletID(detail.UserID, detail.WalletID);

            RequestViewModel list = new RequestViewModel();
            using (var context = new DatabaseContext())
            {
                list = (from overdraftDetail in context.OverdraftDetail
                        where
                        overdraftDetail.OverdraftDetailID == overdraftDetailID &&
                        overdraftDetail.Status == StatusType.Process

                        orderby overdraftDetail.DOC descending

                        select new RequestViewModel
                        {
                            OverdraftDetailID = overdraftDetail.OverdraftDetailID,
                            User = overdraftDetail.User.FirmName.ConcatNameMobileAliasName(overdraftDetail.User.MobileNumber, overdraftDetail.User.AliasName),
                            Amount = overdraftDetail.OverdraftAmount,
                            Wallet = overdraftDetail.Wallet.Name,
                            SenderBalance = SenderBalance,
                            OverdraftBalance = OverdraftBalance,
                            Status = overdraftDetail.Status,
                            UserRemark = overdraftDetail.UserRemark,
                            AdminRemark = overdraftDetail.AdminRemark,
                            DOC = overdraftDetail.DOC.ToString(Messages.DOCDisplayFormat),
                        }).FirstOrDefault();
            }

            return list;
        }

        internal List<ViewModel> SelectOverdraftTransfer(TransferReportRequest selectRequest, int userID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
            {
                list = (from overdraftDetail in context.OverdraftDetail
                        where
                           overdraftDetail.DOCInt >= From
                            && overdraftDetail.DOCInt <= To
                            && overdraftDetail.Type == DOCType.OverdraftTrnasfer
                        orderby overdraftDetail.DOC descending

                        select new ViewModel
                        {
                            OverdraftDetailID = overdraftDetail.OverdraftDetailID,
                            UserID = overdraftDetail.UserID,
                            User = overdraftDetail.User.FirmName.ConcatNameMobileAliasName(overdraftDetail.User.MobileNumber, overdraftDetail.User.AliasName),
                            Amount = overdraftDetail.OverdraftAmount,
                            Wallet = overdraftDetail.Wallet.Name,
                            Status = overdraftDetail.Status,
                            UserRemark = overdraftDetail.UserRemark,
                            AdminRemark = overdraftDetail.AdminRemark,
                            OverdraftOpening = overdraftDetail.OverdraftOpening,
                            OverdraftClosing = overdraftDetail.OverdraftClosing,
                            DOC = overdraftDetail.DOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = overdraftDetail.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            Type = overdraftDetail.Type,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            if (selectRequest.UserID != 0)
            {
                list = (from item in list
                        where item.UserID == selectRequest.UserID
                        select item).ToList();
            }

            return list;
        }

        internal List<ViewModel> SelectOverdraftReverse(TransferReportRequest selectRequest, int userID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
            {
                list = (from overdraftDetail in context.OverdraftDetail
                        where overdraftDetail.DOCInt >= From
                            && overdraftDetail.DOCInt <= To
                            && overdraftDetail.Type == DOCType.OverdraftReverse
                        orderby overdraftDetail.DOC descending

                        select new ViewModel
                        {
                            OverdraftDetailID = overdraftDetail.OverdraftDetailID,
                            UserID = overdraftDetail.UserID,
                            User = overdraftDetail.User.FirmName.ConcatNameMobileAliasName(overdraftDetail.User.MobileNumber, overdraftDetail.User.AliasName),

                            Amount = overdraftDetail.OverdraftAmount,
                            Wallet = overdraftDetail.Wallet.Name,
                            Status = overdraftDetail.Status,
                            UserRemark = overdraftDetail.UserRemark,
                            AdminRemark = overdraftDetail.AdminRemark,
                            OverdraftOpening = overdraftDetail.OverdraftOpening,
                            OverdraftClosing = overdraftDetail.OverdraftClosing,
                            DOC = overdraftDetail.DOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = overdraftDetail.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            Type = overdraftDetail.Type,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }
            if (userID != Company.AdminUserID)
            {
                list = (from item in list
                        where item.UserID == userID
                        select item).ToList();
            }
            if (selectRequest.UserID != 0)
            {
                list = (from item in list
                        where item.UserID == selectRequest.UserID
                        select item).ToList();
            }

            return list;
        }

        internal List<ViewModel> SelectOverdraftReceive(TransferReportRequest selectRequest, int userID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
            {
                list = (from overdraftDetail in context.OverdraftDetail
                        where
                           overdraftDetail.DOCInt >= From
                            && overdraftDetail.DOCInt <= To
                            && overdraftDetail.UserID == userID
                            && overdraftDetail.Type == DOCType.OverdraftTrnasfer
                        orderby overdraftDetail.DOC descending

                        select new ViewModel
                        {
                            OverdraftDetailID = overdraftDetail.OverdraftDetailID,
                            User = overdraftDetail.User.FirmName.ConcatNameMobileAliasName(overdraftDetail.User.MobileNumber, overdraftDetail.User.AliasName),
                            Amount = overdraftDetail.OverdraftAmount,
                            Wallet = overdraftDetail.Wallet.Name,
                            Status = overdraftDetail.Status,
                            UserRemark = overdraftDetail.UserRemark,
                            AdminRemark = overdraftDetail.AdminRemark,
                            OverdraftOpening = overdraftDetail.OverdraftOpening,
                            OverdraftClosing = overdraftDetail.OverdraftClosing,
                            DOC = overdraftDetail.DOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = overdraftDetail.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            Type = overdraftDetail.Type,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }

            return list;
        }

        internal List<ViewModel> SelectOverdraft(TransferReportRequest selectRequest, int userID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
            {
                list = (from overdraftDetail in context.OverdraftDetail
                        where overdraftDetail.DOCInt >= From
                            && overdraftDetail.DOCInt <= To
                            && (overdraftDetail.Type == DOCType.OverdraftReverse
                               || overdraftDetail.Type == DOCType.OverdraftTrnasfer)
                        orderby overdraftDetail.DOC descending

                        select new ViewModel
                        {
                            OverdraftDetailID = overdraftDetail.OverdraftDetailID,
                            User = overdraftDetail.User.FirmName.ConcatNameMobileAliasName(overdraftDetail.User.MobileNumber, overdraftDetail.User.AliasName),
                            UserID = overdraftDetail.UserID,
                            Amount = overdraftDetail.OverdraftAmount,
                            Wallet = overdraftDetail.Wallet.Name,
                            Status = overdraftDetail.Status,
                            UserRemark = overdraftDetail.UserRemark,
                            AdminRemark = overdraftDetail.AdminRemark,
                            OverdraftOpening = overdraftDetail.OverdraftOpening,
                            OverdraftClosing = overdraftDetail.OverdraftClosing,
                            DOC = overdraftDetail.DOC.ToString(Messages.DOCDisplayFormat),
                            UpdatedDOC = overdraftDetail.UpdatedDOC.ToString(Messages.DOCDisplayFormat),
                            Type = overdraftDetail.Type,
                        }).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }
            if (userID != Company.AdminUserID)
            {
                list = (from item in list
                        where item.UserID == userID
                        select item).ToList();
            }
            if (selectRequest.UserID != 0)
            {
                list = (from item in list
                        where item.UserID == selectRequest.UserID
                        select item).ToList();
            }

            return list;
        }

        internal bool IsPending(int UserID)
        {
            using var context = new DatabaseContext();
            OverdraftDetail res = (from item in context.OverdraftDetail
                                   where item.UserID == UserID
                                   && item.Status == StatusType.Process
                                   select item).FirstOrDefault();
            if (res == null)
                return false;
            return true;
        }
    }
}