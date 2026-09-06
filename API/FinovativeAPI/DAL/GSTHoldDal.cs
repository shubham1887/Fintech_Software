using Project.Model;
using Project.Processing;
using Project.Requests.GSTHold;
using Project.Requests.TransactionReport;
using Project.Response.GSTHold;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class GSTHoldDal
    {
        private string Key
        { get { return "_GSTHold"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<GSTHold>>();
            cache.Remove(Key);
        }

        public int Insert(GSTHold gst)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.GSTHold.Add(gst);
            context.SaveChanges();
            return gst.GSTHoldID;
        }

        internal int Update(GSTHold ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.GSTHold.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.GSTHoldID;
            }
        }

        internal List<GSTHold> Select()
        {
            var _newsList = new MemoryCacheProcessing<List<GSTHold>>();
            List<GSTHold> newsList = _newsList.GetOrCreate(Key, () => SelectWithoutCache());
            return newsList;
        }

        internal List<ViewModel> SelectGSTHold(GSTHoldSearchRequest selectRequest, int LoginUserID)
        {
            DateTime FromDate = new DateTime(selectRequest.Year, selectRequest.Month, 1);
            string month = FromDate.ToString("MMMM");
            string monthYearString = month + "-" + selectRequest.Year;

            List<ViewModel> response = new List<ViewModel>();
            List<GSTHold> list = Select();
            List<UserMaster> usersList = new UserDAL().Select();
            List<StateMaster> Statelist = new StateDAL().Select();

            using (var context = new DatabaseContext())
            {
                response = (from item in list
                            join user in usersList on item.UserID equals user.UserID
                            join state in Statelist on item.StateID equals state.ID
                            where item.Month == selectRequest.Month && item.Year == selectRequest.Year
                            select new ViewModel
                            {
                                UserID = item.UserID,
                                GSTHoldID = item.GSTHoldID,
                                FirmName = user.FirmName,
                                AdminRemark = item.AdminRemark,
                                DOC = item.DOC.LongFormatDate(),
                                GSTNo = item.GSTNo,
                                GSTUploadedDate = item.GSTUploadedDate.LongFormatDate(),
                                GSTUploadRemark = item.GSTUploadRemark,
                                HoldAmount = item.HoldAmount,
                                CGST = item.CGST,
                                IGST = item.IGST,
                                SGST = item.SGST,
                                State = state.Name,
                                Month = item.Month,
                                TaxableValue = item.TaxableValue,
                                Year = item.Year,
                                Status = item.Status,
                                LastYearMonth = monthYearString
                            }).ToList();
            }

            response = response.Where(g => g.HoldAmount > 0).ToList();

            if (LoginUserID != Company.AdminUserID && LoginUserID != 0)
            {
                response = (from item in response
                            where item.UserID == LoginUserID
                            select item).ToList();
            }

            return response;
        }

        private List<GSTHold> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.GSTHold.OrderByDescending(gst => gst.DOC).ToList();
        }

        internal List<GSTViewModel> GSTsummary(GSTSummaryRequest selectRequest, string type)
        {
            List<GSTViewModel> list = new List<GSTViewModel>();
            DateTime FromDate = new DateTime(selectRequest.Year, selectRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            string month = FromDate.ToString("MMMM");
            string monthYearString = month + "-" + selectRequest.Year;

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            List<UserMaster> usersList = new UserDAL().Select();
            usersList = usersList.Where(u => u.IsInvoiceApplicable == true &&
            u.GSTHoldCutting == type &&
            u.ParentID == Company.AdminUserID &&
            u.GSTNo.Length > 3).ToList();

            List<TransactionMaster> TXNList = new TransactionDAL().SelectTransactionDatewise(From, To, (int)ServiceCode.MoneyTransfer);

            using (var context = new DatabaseContext())
            {
                var sdList = (
                    from txn in TXNList
                    join user in usersList on txn.SuperDistributerUserID equals user.UserID
                    group txn by new { txn.SuperDistributerUserID } into grp1
                    select new
                    {
                        UserID = grp1.Key.SuperDistributerUserID,
                        Sale = grp1.Sum(x => x.SuperDistributerCommission),
                    }).ToList();

                var distList = (
                    from txn in TXNList
                    join user in usersList on txn.DistributerUserID equals user.UserID
                    group txn by new { txn.DistributerUserID } into grp1
                    select new
                    {
                        UserID = grp1.Key.DistributerUserID,
                        Sale = grp1.Sum(x => x.DistributerCommission),
                    }).ToList();

                var APIList = (from txn in TXNList
                               join user in usersList on txn.UserID equals user.UserID
                               where user.UserTypeID == (int)UserType.APIUser
                               group txn by new { txn.UserID } into grp1
                               select new
                               {
                                   UserID = grp1.Key.UserID,
                                   Sale = grp1.Sum(x => x.RetailerCommission),
                               }).ToList();

                var MainList = distList.Union(sdList).Union(APIList).ToList();

                List<StateMaster> statelist = new StateDAL().Select();

                InvoiceSetting settings = new InvoiceSettingDal().Select();
                list = (from main in MainList
                        join user in usersList on main.UserID equals user.UserID
                        join state in statelist on user.StateID equals state.ID
                        select new GSTViewModel
                        {
                            InvoiceMonth = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                            FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            UserID = user.UserID,
                            GSTNo = user.GSTNo,
                            InvoiceNo = new InvoiceProcessing().GetInvoiceNumber(settings.CompanyPrefix, main.UserID, FromDate.Year, FromDate.Month, "USER"),
                            State = state.Name,
                            StateID = state.ID,
                            InvoiceValue = main.Sale,
                            TaxableValue = main.Sale,
                            SGST = (user.StateID == 1) ? ((main.Sale * 100 / 118) * 9 / 100) : 0,
                            CGST = (user.StateID == 1) ? ((main.Sale * 100 / 118) * 9 / 100) : 0,
                            IGST = (user.StateID != 1) ? ((main.Sale * 100 / 118) * 9 / 100) : 0,
                        }

                     ).ToList();
                list = (from main in list

                        select new GSTViewModel
                        {
                            Year = selectRequest.Year,
                            Month = selectRequest.Month,
                            InvoiceMonth = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                            FirmName = main.FirmName,
                            UserID = main.UserID,
                            GSTNo = main.GSTNo,
                            InvoiceNo = new InvoiceProcessing().GetInvoiceNumber(settings.CompanyPrefix, main.UserID, FromDate.Year, FromDate.Month, "USER"),
                            InvoiceType = "R",
                            State = main.State,
                            StateID = main.StateID,
                            ReverseCharge = 0,
                            Rate = 18,
                            InvoiceValue = main.InvoiceValue,
                            TaxableValue = main.TaxableValue,
                            SGST = main.SGST,
                            CGST = main.CGST,
                            IGST = main.IGST,
                            HoldAmount = (main.StateID == 1) ? main.SGST + main.CGST : main.IGST,
                            LastYearMonth = monthYearString
                        }
                                          ).ToList();
            }

            list = list.Where(g => g.HoldAmount > 0.0001M).ToList();
            return list;
        }

        internal GSTHold SelectByID(GSTViewModel gSTrequest)
        {
            using (var context = new DatabaseContext())
            {
                return context.GSTHold.FirstOrDefault(o => o.UserID == gSTrequest.UserID && o.Month == gSTrequest.Month && o.Year == gSTrequest.Year);
            }
        }

        internal GSTHold SelectByGSTHoldID(int GSTHoldID)
        {
            using (var context = new DatabaseContext())
            {
                return context.GSTHold.FirstOrDefault(o => o.GSTHoldID == GSTHoldID);
            }
        }

        internal decimal SelectHoldAmountUserwise(int UserID, int Month, int Year)
        {
            using (var context = new DatabaseContext())
            {
                GSTHold item = context.GSTHold.FirstOrDefault(o => o.UserID == UserID && o.Month == Month && o.Year == Year && o.Status == StatusType.Process);
                if (item == null) return 0;
                return item.HoldAmount;
            }
        }
    }
}