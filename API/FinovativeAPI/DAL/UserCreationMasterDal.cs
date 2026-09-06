using Project.Model;
using Project.Processing;
using Project.Requests.UserCreationMaster;
using Project.Response.UserCreationMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class UserCreationMasterDal
    {
        internal int Insert(UserCreationMaster model)
        {
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.UserCreationMaster.Add(model);
                context.SaveChanges();
                result = model.UserCreationMasterID;
            }
            return result;
        }

        internal List<ViewModel> Last10Report(int UserID)
        {
            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
            {
                list = (from Uc in context.UserCreationMaster
                        orderby Uc.UserCreationMasterID descending
                        select new ViewModel
                        {
                            UserCreationMasterID = Uc.UserCreationMasterID,
                            FirmName = Uc.UserMaster.FirmName.ConcatNameMobileAliasName(Uc.UserMaster.MobileNumber, Uc.UserMaster.AliasName),
                            UserID = Uc.UserID,
                            OpeningIdCount = Uc.OpeningIdCount,
                            ClosingIdCount = Uc.ClosingIdCount,
                            Amount = Uc.Amount,
                            PurchaseCount = Uc.PurchaseCount,
                            //ClosingIdCount = userparent.IdCount,
                            OpeningBalance = Uc.OpeningBalance,
                            ClosingBalance = Uc.ClosingBalance,
                            DOC = Uc.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            Type = Uc.Type
                        }).ToList();
                if (UserID != Company.AdminUserID)
                {
                    list = (from item in list
                            where item.UserID == UserID
                            && item.Type == IDPurchase.Purchase
                            select item).Take(10).ToList();
                }
                else
                {
                    list = (from item in list
                            where item.Type == IDPurchase.Free
                            select item).Take(10).ToList();
                }

                return list;
            }
        }

        internal List<ViewModel> SelectForFilter(SelectRequest selectRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<UserMaster> list1 = new List<UserMaster>();
            List<ViewModel> list = new List<ViewModel>();
            using (var context = new DatabaseContext())
            {
                list = (from Uc in context.UserCreationMaster
                        where Uc.DOCInt >= FromDate
                            && Uc.DOCInt <= ToDate
                             && (!selectRequest.Type.EqualsIgnoreCase("All") ? Uc.Type == selectRequest.Type : 2 > 1)
                        // && (selectRequest.UserID == 0 ? 2 > 1 : Uc.UserID == selectRequest.UserID)

                        orderby Uc.UserCreationMasterID descending
                        select new ViewModel
                        {
                            UserCreationMasterID = Uc.UserCreationMasterID,
                            FirmName = Uc.UserMaster.FirmName.ConcatNameMobileAliasName(Uc.UserMaster.MobileNumber, Uc.UserMaster.AliasName),
                            UserID = Uc.UserID,
                            OpeningIdCount = Uc.OpeningIdCount,
                            ClosingIdCount = Uc.ClosingIdCount,
                            Amount = Uc.Amount,
                            PurchaseCount = Uc.PurchaseCount,
                            OpeningBalance = Uc.OpeningBalance,
                            ClosingBalance = Uc.ClosingBalance,
                            DOC = Uc.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            Type = Uc.Type
                        }).ToList();
                return list;
            }
        }
    }
}