using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Project.Model;
using Project.Processing;
using Project.Requests.Tax;
using Project.Response.Tax;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class TaxDal
    {
        internal List<GSTDetailViewModel> SelectGSTDetailold(GSTDetailRequest mainRequest, int UserID)
        {
            int UserTypeIDBoth = 0;
            int UserIDForSDDS = 0;

            List<GSTDetailViewModel> list = new List<GSTDetailViewModel>();
            DateTime FromDate = new DateTime(mainRequest.Year, mainRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<Project.Response.Service.ViewModel> services = new ServiceMasterDal().Select();
            List<Project.Response.Operator.ViewModel> operators = new OperatorMasterDAL().Select();
            try
            {
                using var context = new DatabaseContext();

                var mainlist = (from txn in context.TransactionMaster
                                join service in context.ServiceMaster on txn.ServiceID equals service.ServiceID
                                where
                                txn.Status == StatusType.Success &&
                                txn.DOCInt >= From && txn.DOCInt <= To
                                && service.ServiceCategory == mainRequest.ServiceCategory
                                select new
                                {
                                    Amount = txn.Amount,
                                    DebitAmount = txn.DebitAmount,
                                    ServiceID = txn.ServiceID,
                                    ServiceCategory = service.ServiceCategory,
                                    ServiceName = service.Name,
                                    UserID = txn.UserID,
                                    AdminGST = txn.AdminGST,
                                    RetailerCommission = txn.RetailerCommission,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    DistributerUserID = txn.DistributerUserID,
                                }).ToList();

                if (mainRequest.UserID != 0)
                {
                    int SelectedUserTypeID = new UserDAL().SelectUserTypeID(mainRequest.UserID);
                    UserTypeIDBoth = SelectedUserTypeID;
                    UserIDForSDDS = mainRequest.UserID;
                    if (SelectedUserTypeID == (int)UserType.SuperDistributer)
                    {
                        mainlist = (from item in mainlist
                                    where item.SuperDistributerUserID == mainRequest.UserID
                                    select item).ToList();
                    }
                    else if (SelectedUserTypeID == (int)UserType.Distributer)
                    {
                        mainlist = (from item in mainlist
                                    where item.DistributerUserID == mainRequest.UserID
                                    select item).ToList();
                    }
                    else
                    {
                        mainlist = (from item in mainlist
                                    where item.UserID == mainRequest.UserID
                                    select item).ToList();
                    }
                }

                if (!Util.isRoleAdminOrSupport(UserTypeID))
                {
                    UserTypeIDBoth = UserTypeID;
                    UserIDForSDDS = UserID;
                    if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        mainlist = (from item in mainlist
                                    where item.SuperDistributerUserID == mainRequest.UserID
                                    select item).ToList();
                    }
                    else if (UserTypeID == (int)UserType.Distributer)
                    {
                        mainlist = (from item in mainlist
                                    where item.DistributerUserID == mainRequest.UserID
                                    select item).ToList();
                    }
                    else
                    {
                        mainlist = (from item in mainlist
                                    where item.UserID == mainRequest.UserID
                                    select item).ToList();
                    }
                }

                List<GSTMiddleView> result = new List<GSTMiddleView>();

                if (UserTypeIDBoth == (int)UserType.Distributer)
                {
                    result = (from item in mainlist

                              group item by new { item.ServiceCategory, item.DistributerUserID } into grp
                              select new GSTMiddleView
                              {
                                  ServiceCategory = grp.Key.ServiceCategory,
                                  UserID = grp.Key.DistributerUserID,
                                  Qty = grp.Sum(tr => tr.Amount),
                                  DiscountAmount = grp.Sum(tr => tr.DistributerCommission),
                                  SuccessCount = grp.Count(),
                              }).ToList();

                    result = (from item in result
                              where item.UserID == UserIDForSDDS
                              select item).ToList();
                }
                else if (UserTypeIDBoth == (int)UserType.SuperDistributer)
                {
                    result = (from item in mainlist
                              group item by new { item.ServiceCategory, item.SuperDistributerUserID } into grp
                              select new GSTMiddleView
                              {
                                  ServiceCategory = grp.Key.ServiceCategory,
                                  UserID = grp.Key.SuperDistributerUserID,
                                  Qty = grp.Sum(tr => tr.Amount),
                                  DiscountAmount = grp.Sum(tr => tr.SuperDistributerCommission),
                                  SuccessCount = grp.Count(),
                              }).ToList();

                    result = (from item in result
                              where item.UserID == UserIDForSDDS
                              select item).ToList();
                }
                else if (Util.isRoleAdminOrSupport(UserTypeID) && UserTypeIDBoth == 0)
                {
                    List<GSTMiddleView> resultsd = new List<GSTMiddleView>();
                    resultsd = (from item in mainlist
                                group item by new { item.ServiceCategory, item.SuperDistributerUserID } into grp
                                select new GSTMiddleView
                                {
                                    ServiceCategory = grp.Key.ServiceCategory,
                                    UserID = grp.Key.SuperDistributerUserID,
                                    Qty = grp.Sum(tr => tr.Amount),
                                    DiscountAmount = grp.Sum(tr => tr.SuperDistributerCommission),
                                    SuccessCount = grp.Count(),
                                }).ToList();

                    List<GSTMiddleView> resultds = new List<GSTMiddleView>();
                    resultds = (from item in mainlist

                                group item by new { item.ServiceCategory, item.DistributerUserID } into grp
                                select new GSTMiddleView
                                {
                                    ServiceCategory = grp.Key.ServiceCategory,
                                    UserID = grp.Key.DistributerUserID,
                                    Qty = grp.Sum(tr => tr.Amount),
                                    DiscountAmount = grp.Sum(tr => tr.DistributerCommission),
                                    SuccessCount = grp.Count(),
                                }).ToList();

                    List<GSTMiddleView> resultret = new List<GSTMiddleView>();
                    resultret = (from item in mainlist
                                 group item by new { item.ServiceCategory, item.UserID } into grp
                                 select new GSTMiddleView
                                 {
                                     ServiceCategory = grp.Key.ServiceCategory,
                                     UserID = grp.Key.UserID,
                                     Qty = grp.Sum(tr => tr.Amount),
                                     DiscountAmount = grp.Sum(tr => tr.RetailerCommission),
                                     SuccessCount = grp.Count(),
                                 }).ToList();

                    result = resultsd.Union(resultsd).ToList();
                    result = result.Union(resultds).ToList();
                    result = result.Union(resultret).ToList();
                }
                else
                {
                    result = (from item in mainlist
                              group item by new { item.ServiceCategory, item.UserID } into grp
                              select new GSTMiddleView
                              {
                                  ServiceCategory = grp.Key.ServiceCategory,
                                  UserID = grp.Key.UserID,
                                  Qty = grp.Sum(tr => tr.Amount),
                                  DiscountAmount = grp.Sum(tr => tr.RetailerCommission),
                                  SuccessCount = grp.Count(),
                              }).ToList();
                }

                List<GSTListViewModel> GSTlist = new List<GSTListViewModel>();

                List<UserMaster> usersList = new UserDAL().Select();

                var usersListgst = (from mainuser in usersList

                                    select new
                                    {
                                        GSTType = mainuser?.GSTPrinciple ?? GSTTypes.Inclusion,
                                        UserID = mainuser.UserID,
                                    }).ToList();

                int AdminstateID = (from item in usersList
                                    where item.UserID == Company.AdminUserID
                                    select item.StateID).FirstOrDefault();

                if (TaxInfo.Prepaid.EqualsIgnoreCase(mainRequest.ServiceCategory))
                {
                    var GSTTaxable = (from main in result
                                      join user in usersListgst on main.UserID equals user.UserID
                                      select new
                                      {
                                          GSTType = user.GSTType,
                                          UserID = main.UserID,
                                          GSTTaxableValue = ((main.Qty - main.DiscountAmount) / 1.18M) * 0.18M,
                                          TaxableValue = ((main.Qty - main.DiscountAmount) / 1.18M)
                                      }).ToList();

                    GSTlist = (from main in GSTTaxable
                               join user in usersList on main.UserID equals user.UserID
                               select new GSTListViewModel
                               {
                                   GSTTaxableValue = main.GSTTaxableValue,
                                   TaxableValue = main.TaxableValue,
                                   GSTType = main.GSTType,
                                   UserID = main.UserID,
                                   SGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   CGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   IGST = (user.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                               }).ToList();
                }

                if (TaxInfo.Prepaid.EqualsIgnoreCase(mainRequest.ServiceCategory)
                    && (UserTypeIDBoth == (int)UserType.Distributer || UserTypeIDBoth == (int)UserType.SuperDistributer))
                {
                    var GSTTaxableExclusion = (from main in result
                                               join user in usersListgst on main.UserID equals user.UserID
                                               where user.GSTType == GSTTypes.Exclusion
                                               select new
                                               {
                                                   GSTType = user.GSTType,
                                                   UserID = main.UserID,
                                                   GSTTaxableValue = main.DiscountAmount * 0.18M,
                                                   TaxableValue = main.DiscountAmount,
                                               }).ToList();

                    var GSTTaxableInclusion = (from main in result
                                               join user in usersListgst on main.UserID equals user.UserID
                                               where user.GSTType == GSTTypes.Inclusion
                                               select new
                                               {
                                                   GSTType = user.GSTType,
                                                   UserID = main.UserID,
                                                   GSTTaxableValue = (main.DiscountAmount / 1.18M) * 0.18M,
                                                   TaxableValue = main.DiscountAmount,
                                               }).ToList();

                    var GSTTaxable = GSTTaxableExclusion.Union(GSTTaxableInclusion).ToList();

                    GSTlist = (from main in GSTTaxable
                               join user in usersList on main.UserID equals user.UserID
                               select new GSTListViewModel
                               {
                                   GSTTaxableValue = main.GSTTaxableValue,
                                   TaxableValue = main.TaxableValue,
                                   GSTType = main.GSTType,
                                   UserID = main.UserID,
                                   SGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   CGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   IGST = (user.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                               }).ToList();
                }

                if ((TaxInfo.DMT.EqualsIgnoreCase(mainRequest.ServiceCategory)) ||
                    TaxInfo.Utility.EqualsIgnoreCase(mainRequest.ServiceCategory))
                {
                    var GSTTaxableExclusion = (from main in result
                                               join user in usersListgst on main.UserID equals user.UserID
                                               where user.GSTType == GSTTypes.Exclusion
                                               select new
                                               {
                                                   GSTType = user.GSTType,
                                                   UserID = main.UserID,
                                                   GSTTaxableValue = main.DiscountAmount * 0.18M,
                                                   TaxableValue = main.DiscountAmount,
                                               }).ToList();

                    var GSTTaxableInclusion = (from main in result
                                               join user in usersListgst on main.UserID equals user.UserID
                                               where user.GSTType == GSTTypes.Inclusion
                                               select new
                                               {
                                                   GSTType = user.GSTType,
                                                   UserID = main.UserID,
                                                   GSTTaxableValue = (main.DiscountAmount / 1.18M) * 0.18M,
                                                   TaxableValue = (main.DiscountAmount / 1.18M),
                                               }).ToList();

                    var GSTTaxable = GSTTaxableExclusion.Union(GSTTaxableInclusion).ToList();

                    GSTlist = (from main in GSTTaxable
                               join user in usersList on main.UserID equals user.UserID
                               select new GSTListViewModel
                               {
                                   GSTTaxableValue = main.GSTTaxableValue,
                                   TaxableValue = main.TaxableValue,
                                   UserID = main.UserID,
                                   GSTType = main.GSTType,
                                   SGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   CGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   IGST = (user.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                               }).ToList();
                }

                List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
                List<StateMaster> statelist = new StateDAL().Select();

                list = (from main in result

                        join gst in GSTlist on main.UserID equals gst.UserID
                        join user in usersList on main.UserID equals user.UserID
                        join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                        join state in statelist on user.StateID equals state.ID
                        orderby usertype.UserTypeID, user.UserID

                        select new GSTDetailViewModel
                        {
                            GSTType = gst.GSTType,
                            ServiceCategory = main.ServiceCategory,
                            InvoiceMonth = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                            FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            UserID = user.UserID,
                            MobileNumber = user.MobileNumber,
                            GSTNo = user.GSTNo,
                            State = state.Name,
                            StateID = state.ID,
                            SuccessAmt = main.Qty.TwoDigitDecimalValue(),
                            SuccessCount = main.SuccessCount,
                            DiscountAmount = main.DiscountAmount.TwoDigitDecimalValue(),
                            IGST = Math.Abs(gst.IGST.TwoDigitDecimalValue()),
                            SGST = Math.Abs(gst.SGST.TwoDigitDecimalValue()),
                            CGST = Math.Abs(gst.CGST.TwoDigitDecimalValue()),
                            TaxableValue = Math.Abs(gst.TaxableValue.TwoDigitDecimalValue()),
                            GSTTaxableValue = Math.Abs(gst.GSTTaxableValue.TwoDigitDecimalValue()),
                            UserType = usertype.UserType,
                        }).ToList();

                list = list.Select((o, i) => { o.SequenceNumber = i + 1; return o; }).ToList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        internal List<GSTMonthlyViewModel> SelectTransactionDatewise(GSTDetailRequest mainRequest, int UserID, out int UserTypeIDBoth, out int UserIDForSDDS)
        {
            DateTime FromDate = new DateTime(mainRequest.Year, mainRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();
            UserTypeIDBoth = 0;
            UserIDForSDDS = 0;
            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));
            List<GSTMonthlyViewModel> mainlist = new List<GSTMonthlyViewModel>();
            using var context = new DatabaseContext();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            mainlist = (from txn in context.TransactionMaster
                        join service in context.ServiceMaster on txn.ServiceID equals service.ServiceID
                        where
                        txn.Status == StatusType.Success &&
                        txn.DOCInt >= From && txn.DOCInt <= To
                        && service.ServiceCategory == mainRequest.ServiceCategory
                        select new GSTMonthlyViewModel
                        {
                            Amount = txn.Amount,
                            DebitAmount = txn.DebitAmount,
                            ServiceID = txn.ServiceID,
                            ServiceCategory = service.ServiceCategory,
                            ServiceName = service.Name,
                            UserID = txn.UserID,
                            AdminGST = txn.AdminGST,
                            RetailerCommission = txn.RetailerCommission,
                            DistributerCommission = txn.DistributerCommission,
                            SuperDistributerCommission = txn.SuperDistributerCommission,
                            SuperDistributerUserID = txn.SuperDistributerUserID,
                            DistributerUserID = txn.DistributerUserID,
                            IsCCF = txn.IsCCF,
                            CashBack = txn.CashBack
                        }).ToList();

            if (mainRequest.UserID != 0)
            {
                int SelectedUserTypeID = new UserDAL().SelectUserTypeID(mainRequest.UserID);
                UserTypeIDBoth = SelectedUserTypeID;
                UserIDForSDDS = mainRequest.UserID;
                if (SelectedUserTypeID == (int)UserType.SuperDistributer)
                {
                    mainlist = (from item in mainlist
                                where item.SuperDistributerUserID == mainRequest.UserID
                                select item).ToList();
                }
                else if (SelectedUserTypeID == (int)UserType.Distributer)
                {
                    mainlist = (from item in mainlist
                                where item.DistributerUserID == mainRequest.UserID
                                select item).ToList();
                }
                else
                {
                    mainlist = (from item in mainlist
                                where item.UserID == mainRequest.UserID
                                select item).ToList();
                }
            }

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                UserTypeIDBoth = UserTypeID;
                UserIDForSDDS = UserID;
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    mainlist = (from item in mainlist
                                where item.SuperDistributerUserID == mainRequest.UserID
                                select item).ToList();
                }
                else if (UserTypeID == (int)UserType.Distributer)
                {
                    mainlist = (from item in mainlist
                                where item.DistributerUserID == mainRequest.UserID
                                select item).ToList();
                }
                else
                {
                    mainlist = (from item in mainlist
                                where item.UserID == mainRequest.UserID
                                select item).ToList();
                }
            }

            return mainlist;
        }

        internal List<GSTMonthlyViewModel> SelectTransactionDatewiseWithGST(GSTDetailRequest mainRequest, int UserID, out int UserTypeIDBoth, out int UserIDForSDDS)
        {
            DateTime FromDate = new DateTime(mainRequest.Year, mainRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();
            UserTypeIDBoth = 0;
            UserIDForSDDS = 0;
            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));
            List<GSTMonthlyViewModel> mainlist = new List<GSTMonthlyViewModel>();
            using var context = new DatabaseContext();

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            mainlist = (from txn in context.TransactionMaster
                        join service in context.ServiceMaster on txn.ServiceID equals service.ServiceID
                        where
                        txn.Status == StatusType.Success &&
                        txn.DOCInt >= From && txn.DOCInt <= To
                        && service.ServiceCategory == mainRequest.ServiceCategory
                        select new GSTMonthlyViewModel
                        {
                            Amount = txn.Amount,
                            DebitAmount = txn.DebitAmount,
                            ServiceID = txn.ServiceID,
                            ServiceCategory = service.ServiceCategory,
                            ServiceName = service.Name,
                            UserID = txn.UserID,
                            AdminGST = txn.AdminGST,
                            RetailerCommission = txn.RetailerCommission,
                            DistributerCommission = txn.DistributerCommission,
                            SuperDistributerCommission = txn.SuperDistributerCommission,
                            SuperDistributerUserID = txn.SuperDistributerUserID,
                            DistributerUserID = txn.DistributerUserID,
                            RetailerGST = txn.RetailerGST,
                            DistributerGST = txn.DistributerGST,
                            SuperDistributerGST = txn.SuperDistributerGST,
                        }).ToList();

            if (mainRequest.UserID != 0)
            {
                int SelectedUserTypeID = new UserDAL().SelectUserTypeID(mainRequest.UserID);
                UserTypeIDBoth = SelectedUserTypeID;
                UserIDForSDDS = mainRequest.UserID;
                if (SelectedUserTypeID == (int)UserType.SuperDistributer)
                {
                    mainlist = (from item in mainlist
                                where item.SuperDistributerUserID == mainRequest.UserID
                                select item).ToList();
                }
                else if (SelectedUserTypeID == (int)UserType.Distributer)
                {
                    mainlist = (from item in mainlist
                                where item.DistributerUserID == mainRequest.UserID
                                select item).ToList();
                }
                else
                {
                    mainlist = (from item in mainlist
                                where item.UserID == mainRequest.UserID
                                select item).ToList();
                }
            }

            if (!Util.isRoleAdminOrSupport(UserTypeID))
            {
                UserTypeIDBoth = UserTypeID;
                UserIDForSDDS = UserID;
                if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    mainlist = (from item in mainlist
                                where item.SuperDistributerUserID == mainRequest.UserID
                                select item).ToList();
                }
                else if (UserTypeID == (int)UserType.Distributer)
                {
                    mainlist = (from item in mainlist
                                where item.DistributerUserID == mainRequest.UserID
                                select item).ToList();
                }
                else
                {
                    mainlist = (from item in mainlist
                                where item.UserID == mainRequest.UserID
                                select item).ToList();
                }
            }

            return mainlist;
        }

        internal List<GSTMiddleView> SelectGroupWise(List<GSTMonthlyViewModel> mainlist, int UserTypeID, int UserTypeIDBoth, int UserIDForSDDS)
        {
            List<GSTMiddleView> result = new List<GSTMiddleView>();
            if (UserTypeIDBoth == (int)UserType.Distributer)
            {
                result = (from item in mainlist

                          group item by new { item.ServiceCategory, item.DistributerUserID } into grp
                          select new GSTMiddleView
                          {
                              ServiceCategory = grp.Key.ServiceCategory,
                              UserID = grp.Key.DistributerUserID,
                              Qty = grp.Sum(tr => tr.Amount),
                              DiscountAmount = grp.Sum(tr => tr.DistributerCommission),
                              SuccessCount = grp.Count(),
                          }).ToList();

                result = (from item in result
                          where item.UserID == UserIDForSDDS
                          select item).ToList();
            }
            else if (UserTypeIDBoth == (int)UserType.SuperDistributer)
            {
                result = (from item in mainlist
                          group item by new { item.ServiceCategory, item.SuperDistributerUserID } into grp
                          select new GSTMiddleView
                          {
                              ServiceCategory = grp.Key.ServiceCategory,
                              UserID = grp.Key.SuperDistributerUserID,
                              Qty = grp.Sum(tr => tr.Amount),
                              DiscountAmount = grp.Sum(tr => tr.SuperDistributerCommission),
                              SuccessCount = grp.Count(),
                          }).ToList();

                result = (from item in result
                          where item.UserID == UserIDForSDDS
                          select item).ToList();
            }
            else if (Util.isRoleAdminOrSupport(UserTypeID) && UserTypeIDBoth == 0)
            {
                List<GSTMiddleView> resultsd = new List<GSTMiddleView>();
                resultsd = (from item in mainlist
                            group item by new { item.ServiceCategory, item.SuperDistributerUserID } into grp
                            select new GSTMiddleView
                            {
                                ServiceCategory = grp.Key.ServiceCategory,
                                UserID = grp.Key.SuperDistributerUserID,
                                Qty = grp.Sum(tr => tr.Amount),
                                DiscountAmount = grp.Sum(tr => tr.SuperDistributerCommission),
                                SuccessCount = grp.Count(),
                            }).ToList();

                List<GSTMiddleView> resultds = new List<GSTMiddleView>();
                resultds = (from item in mainlist

                            group item by new { item.ServiceCategory, item.DistributerUserID } into grp
                            select new GSTMiddleView
                            {
                                ServiceCategory = grp.Key.ServiceCategory,
                                UserID = grp.Key.DistributerUserID,
                                Qty = grp.Sum(tr => tr.Amount),
                                DiscountAmount = grp.Sum(tr => tr.DistributerCommission),
                                SuccessCount = grp.Count(),
                            }).ToList();

                List<GSTMiddleView> resultret = new List<GSTMiddleView>();
                resultret = (from item in mainlist
                             group item by new { item.ServiceCategory, item.UserID } into grp
                             select new GSTMiddleView
                             {
                                 ServiceCategory = grp.Key.ServiceCategory,
                                 UserID = grp.Key.UserID,
                                 Qty = grp.Where(tr => tr.IsCCF == false).Sum(tr => tr.Amount),
                                 CCFQty= grp.Where(tr => tr.IsCCF == true).Sum(tr => tr.Amount),
                                 DiscountAmount = grp.Where(tr => tr.IsCCF == false).Sum(tr => tr.RetailerCommission),
                                 SuccessCount = grp.Count(),
                                 CashBack = grp.Sum(tr => tr.CashBack)
                             }).ToList();

                result = resultsd.Union(resultsd).ToList();
                result = result.Union(resultds).ToList();
                result = result.Union(resultret).ToList();
            }
            else
            {
                result = (from item in mainlist
                          group item by new { item.ServiceCategory, item.UserID } into grp
                          select new GSTMiddleView
                          {
                              ServiceCategory = grp.Key.ServiceCategory,
                              UserID = grp.Key.UserID,
                              Qty = grp.Where(tr => tr.IsCCF == false).Sum(tr => tr.Amount),
                              CCFQty = grp.Where(tr => tr.IsCCF == true).Sum(tr => tr.Amount),
                              DiscountAmount = grp.Where(tr => tr.IsCCF==false).Sum(tr => tr.RetailerCommission),
                              SuccessCount = grp.Count(),
                              CashBack = grp.Sum(tr => tr.CashBack)
                          }).ToList();
            }

            return result;
        }

        internal List<GSTMiddleView> SelectGroupWiseByTrsansction(List<GSTMonthlyViewModel> mainlist, int UserTypeID, int UserTypeIDBoth, int UserIDForSDDS)
        {
            List<GSTMiddleView> result = new List<GSTMiddleView>();
            if (UserTypeIDBoth == (int)UserType.Distributer)
            {
                result = (from item in mainlist

                          group item by new { item.ServiceCategory, item.DistributerUserID } into grp
                          select new GSTMiddleView
                          {
                              ServiceCategory = grp.Key.ServiceCategory,
                              UserID = grp.Key.DistributerUserID,
                              Qty = grp.Sum(tr => tr.Amount),
                              DiscountAmount = grp.Sum(tr => tr.DistributerCommission),
                              GST = grp.Sum(tr => tr.DistributerGST),
                              SuccessCount = grp.Count(),
                          }).ToList();

                result = (from item in result
                          where item.UserID == UserIDForSDDS
                          select item).ToList();
            }
            else if (UserTypeIDBoth == (int)UserType.SuperDistributer)
            {
                result = (from item in mainlist
                          group item by new { item.ServiceCategory, item.SuperDistributerUserID } into grp
                          select new GSTMiddleView
                          {
                              ServiceCategory = grp.Key.ServiceCategory,
                              UserID = grp.Key.SuperDistributerUserID,
                              Qty = grp.Sum(tr => tr.Amount),
                              DiscountAmount = grp.Sum(tr => tr.SuperDistributerCommission),
                              GST = grp.Sum(tr => tr.SuperDistributerGST),
                              SuccessCount = grp.Count(),
                          }).ToList();

                result = (from item in result
                          where item.UserID == UserIDForSDDS
                          select item).ToList();
            }
            else if (Util.isRoleAdminOrSupport(UserTypeID) && UserTypeIDBoth == 0)
            {
                List<GSTMiddleView> resultsd = new List<GSTMiddleView>();
                resultsd = (from item in mainlist
                            group item by new { item.ServiceCategory, item.SuperDistributerUserID } into grp
                            select new GSTMiddleView
                            {
                                ServiceCategory = grp.Key.ServiceCategory,
                                UserID = grp.Key.SuperDistributerUserID,
                                Qty = grp.Sum(tr => tr.Amount),
                                DiscountAmount = grp.Sum(tr => tr.SuperDistributerCommission),
                                GST = grp.Sum(tr => tr.SuperDistributerGST),
                                SuccessCount = grp.Count(),
                            }).ToList();

                List<GSTMiddleView> resultds = new List<GSTMiddleView>();
                resultds = (from item in mainlist

                            group item by new { item.ServiceCategory, item.DistributerUserID } into grp
                            select new GSTMiddleView
                            {
                                ServiceCategory = grp.Key.ServiceCategory,
                                UserID = grp.Key.DistributerUserID,
                                Qty = grp.Sum(tr => tr.Amount),
                                DiscountAmount = grp.Sum(tr => tr.DistributerCommission),
                                GST = grp.Sum(tr => tr.DistributerGST),
                                SuccessCount = grp.Count(),
                            }).ToList();

                List<GSTMiddleView> resultret = new List<GSTMiddleView>();
                resultret = (from item in mainlist
                             group item by new { item.ServiceCategory, item.UserID } into grp
                             select new GSTMiddleView
                             {
                                 ServiceCategory = grp.Key.ServiceCategory,
                                 UserID = grp.Key.UserID,
                                 Qty = grp.Sum(tr => tr.Amount),
                                 DiscountAmount = grp.Sum(tr => tr.RetailerCommission),
                                 GST = grp.Sum(tr => tr.RetailerGST),
                                 SuccessCount = grp.Count(),
                             }).ToList();

                result = resultsd.Union(resultsd).ToList();
                result = result.Union(resultds).ToList();
                result = result.Union(resultret).ToList();
            }
            else
            {
                result = (from item in mainlist
                          group item by new { item.ServiceCategory, item.UserID } into grp
                          select new GSTMiddleView
                          {
                              ServiceCategory = grp.Key.ServiceCategory,
                              UserID = grp.Key.UserID,
                              Qty = grp.Sum(tr => tr.Amount),
                              DiscountAmount = grp.Sum(tr => tr.RetailerCommission),
                              GST = grp.Sum(tr => tr.RetailerGST),
                              SuccessCount = grp.Count(),
                          }).ToList();
            }

            return result;
        }

        internal List<GSTListViewModel> SelectGSTList(List<GSTMiddleView> result, GSTDetailRequest mainRequest, List<UserMaster> usersList)
        {
            List<GSTListViewModel> GSTlist = new List<GSTListViewModel>();
            var usersListgst = (from mainuser in usersList

                                select new
                                {
                                    GSTType = mainuser?.GSTPrinciple ?? GSTTypes.Inclusion,
                                    UserID = mainuser.UserID,
                                }).ToList();

            int AdminstateID = (from item in usersList
                                where item.UserID == Company.AdminUserID
                                select item.StateID).FirstOrDefault();

            if (TaxInfo.Prepaid.EqualsIgnoreCase(mainRequest.ServiceCategory))
            {
                var GSTTaxable = (from main in result
                                  join user in usersListgst on main.UserID equals user.UserID
                                  select new
                                  {
                                      GSTType = user.GSTType,
                                      UserID = main.UserID,
                                      GSTTaxableValue = ((main.Qty - main.DiscountAmount) / 1.18M) * 0.18M,
                                      TaxableValue = ((main.Qty - main.DiscountAmount) / 1.18M)
                                  }).ToList();

                GSTlist = (from main in GSTTaxable
                           join user in usersList on main.UserID equals user.UserID
                           select new GSTListViewModel
                           {
                               GSTTaxableValue = main.GSTTaxableValue,
                               TaxableValue = main.TaxableValue,
                               GSTType = main.GSTType,
                               UserID = main.UserID,
                               SGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                               CGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                               IGST = (user.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                           }).ToList();
            }

            if (TaxInfo.Prepaid.EqualsIgnoreCase(mainRequest.ServiceCategory) && TaxInfo.SelfInvoice.EqualsIgnoreCase(mainRequest.InvoiceType))

            //  && (UserTypeIDBoth == (int)UserType.Distributer || UserTypeIDBoth == (int)UserType.SuperDistributer))
            {
                var GSTTaxableExclusion = (from main in result
                                           join user in usersListgst on main.UserID equals user.UserID
                                           where user.GSTType == GSTTypes.Exclusion
                                           select new
                                           {
                                               GSTType = user.GSTType,
                                               UserID = main.UserID,
                                               GSTTaxableValue = main.DiscountAmount * 0.18M,
                                               TaxableValue = main.DiscountAmount,
                                           }).ToList();

                var GSTTaxableInclusion = (from main in result
                                           join user in usersListgst on main.UserID equals user.UserID
                                           where user.GSTType == GSTTypes.Inclusion
                                           select new
                                           {
                                               GSTType = user.GSTType,
                                               UserID = main.UserID,
                                               GSTTaxableValue = (main.DiscountAmount / 1.18M) * 0.18M,
                                               TaxableValue = main.DiscountAmount,
                                           }).ToList();

                var GSTTaxable = GSTTaxableExclusion.Union(GSTTaxableInclusion).ToList();

                GSTlist = (from main in GSTTaxable
                           join user in usersList on main.UserID equals user.UserID
                           select new GSTListViewModel
                           {
                               GSTTaxableValue = main.GSTTaxableValue,
                               TaxableValue = main.TaxableValue,
                               GSTType = main.GSTType,
                               UserID = main.UserID,
                               SGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                               CGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                               IGST = (user.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                           }).ToList();
            }

            if ((TaxInfo.DMT.EqualsIgnoreCase(mainRequest.ServiceCategory)) ||
                TaxInfo.Utility.EqualsIgnoreCase(mainRequest.ServiceCategory))
            {
                var GSTTaxableExclusion = (from main in result
                                           join user in usersListgst on main.UserID equals user.UserID
                                           where user.GSTType == GSTTypes.Exclusion
                                           select new
                                           {
                                               GSTType = user.GSTType,
                                               UserID = main.UserID,
                                               GSTTaxableValue = main.DiscountAmount * 0.18M,
                                               TaxableValue = main.DiscountAmount,
                                               CCFQty = main.CCFQty * 0.01M,
                                           }).ToList();

                var GSTTaxableInclusion = (from main in result
                                           join user in usersListgst on main.UserID equals user.UserID
                                           where user.GSTType == GSTTypes.Inclusion
                                           select new
                                           {
                                               GSTType = user.GSTType,
                                               UserID = main.UserID,
                                               GSTTaxableValue = (main.DiscountAmount / 1.18M) * 0.18M,
                                               TaxableValue = (main.DiscountAmount / 1.18M),
                                               CCFQty = main.CCFQty *0.01M,
                                           }).ToList();

                var GSTTaxable = GSTTaxableExclusion.Union(GSTTaxableInclusion).ToList();

                GSTlist = (from main in GSTTaxable
                           join user in usersList on main.UserID equals user.UserID
                           select new GSTListViewModel
                           {
                               GSTTaxableValue = main.GSTTaxableValue,
                               TaxableValue = main.TaxableValue,
                               UserID = main.UserID,
                               GSTType = main.GSTType,
                               SGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                               CGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                               IGST = (user.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                               CCF=main.CCFQty
                              
                           }).ToList();
            }

            return GSTlist;
        }

        private List<GSTListViewModel> SelectGSTListByTrsansction(List<GSTMiddleView> result, GSTDetailRequest mainRequest, List<UserMaster> usersList)
        {
            List<GSTListViewModel> GSTlist = new List<GSTListViewModel>();

            var usersListgst = (from mainuser in usersList

                                select new
                                {
                                    GSTType = mainuser?.GSTPrinciple ?? GSTTypes.Inclusion,
                                    UserID = mainuser.UserID,
                                }).ToList();

            int AdminstateID = (from item in usersList
                                where item.UserID == Company.AdminUserID
                                select item.StateID).FirstOrDefault();

            if (TaxInfo.P2P.EqualsIgnoreCase(mainRequest.ServiceCategory))
            {
                var GSTTaxable = (from main in result
                                  join user in usersListgst on main.UserID equals user.UserID
                                  select new
                                  {
                                      GSTType = user.GSTType,
                                      UserID = main.UserID,
                                      GSTTaxableValue = ((main.Qty - main.DiscountAmount) / 1.18M) * 0.18M,
                                      TaxableValue = ((main.Qty - main.DiscountAmount) / 1.18M)
                                  }).ToList();

                GSTlist = (from main in GSTTaxable
                           join user in usersList on main.UserID equals user.UserID
                           select new GSTListViewModel
                           {
                               GSTTaxableValue = main.GSTTaxableValue,
                               TaxableValue = main.TaxableValue,
                               GSTType = main.GSTType,
                               UserID = main.UserID,
                               SGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                               CGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                               IGST = (user.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                           }).ToList();
            }

            if (TaxInfo.P2P.EqualsIgnoreCase(mainRequest.ServiceCategory) && TaxInfo.SelfInvoice.EqualsIgnoreCase(mainRequest.InvoiceType))

            //  && (UserTypeIDBoth == (int)UserType.Distributer || UserTypeIDBoth == (int)UserType.SuperDistributer))
            {
                var GSTTaxableExclusion = (from main in result
                                           join user in usersListgst on main.UserID equals user.UserID
                                           where user.GSTType == GSTTypes.Exclusion
                                           select new
                                           {
                                               GSTType = user.GSTType,
                                               UserID = main.UserID,
                                               GSTTaxableValue = main.DiscountAmount * 0.18M,
                                               TaxableValue = main.DiscountAmount,
                                           }).ToList();

                var GSTTaxableInclusion = (from main in result
                                           join user in usersListgst on main.UserID equals user.UserID
                                           where user.GSTType == GSTTypes.Inclusion
                                           select new
                                           {
                                               GSTType = user.GSTType,
                                               UserID = main.UserID,
                                               GSTTaxableValue = (main.DiscountAmount / 1.18M) * 0.18M,
                                               TaxableValue = main.DiscountAmount,
                                           }).ToList();

                var GSTTaxable = GSTTaxableExclusion.Union(GSTTaxableInclusion).ToList();

                GSTlist = (from main in GSTTaxable
                           join user in usersList on main.UserID equals user.UserID
                           select new GSTListViewModel
                           {
                               GSTTaxableValue = main.GSTTaxableValue,
                               TaxableValue = main.TaxableValue,
                               GSTType = main.GSTType,
                               UserID = main.UserID,
                               SGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                               CGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                               IGST = (user.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                           }).ToList();
            }

            if (TaxInfo.P2ABankingRCM.EqualsIgnoreCase(mainRequest.ServiceCategory) ||
                TaxInfo.P2ABankingNonRCM.EqualsIgnoreCase(mainRequest.ServiceCategory) ||
                TaxInfo.P2ANonBanking.EqualsIgnoreCase(mainRequest.ServiceCategory))
            {
                var GSTTaxableExclusion = (from main in result
                                           join user in usersListgst on main.UserID equals user.UserID
                                           where user.GSTType == GSTTypes.Exclusion
                                           select new
                                           {
                                               GSTType = user.GSTType,
                                               UserID = main.UserID,
                                               GSTTaxableValue = main.DiscountAmount * 0.18M,
                                               TaxableValue = main.DiscountAmount,
                                           }).ToList();

                var GSTTaxableInclusion = (from main in result
                                           join user in usersListgst on main.UserID equals user.UserID
                                           where user.GSTType == GSTTypes.Inclusion
                                           select new
                                           {
                                               GSTType = user.GSTType,
                                               UserID = main.UserID,
                                               GSTTaxableValue = (main.DiscountAmount / 1.18M) * 0.18M,
                                               TaxableValue = (main.DiscountAmount / 1.18M),
                                           }).ToList();

                var GSTTaxable = GSTTaxableExclusion.Union(GSTTaxableInclusion).ToList();

                GSTlist = (from main in GSTTaxable
                           join user in usersList on main.UserID equals user.UserID
                           select new GSTListViewModel
                           {
                               GSTTaxableValue = main.GSTTaxableValue,
                               TaxableValue = main.TaxableValue,
                               UserID = main.UserID,
                               GSTType = main.GSTType,
                               SGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                               CGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                               IGST = (user.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                           }).ToList();
            }

            return GSTlist;
        }

        internal List<GSTDetailViewModel> SelectGSTDetail(GSTDetailRequest mainRequest, int UserID)
        {
            int UserTypeIDBoth = 0;
            int UserIDForSDDS = 0;

            List<GSTDetailViewModel> list = new List<GSTDetailViewModel>();
            DateTime FromDate = new DateTime(mainRequest.Year, mainRequest.Month, 1);

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<Project.Response.Service.ViewModel> services = new ServiceMasterDal().Select();
            List<Project.Response.Operator.ViewModel> operators = new OperatorMasterDAL().Select();
            try
            {
                List<GSTMonthlyViewModel> mainlist = SelectTransactionDatewise(mainRequest, UserID, out UserTypeIDBoth, out UserIDForSDDS);

                List<GSTMiddleView> result = SelectGroupWise(mainlist, UserTypeID, UserTypeIDBoth, UserIDForSDDS);

                List<UserMaster> usersList = new UserDAL().Select();

                List<GSTListViewModel> GSTlist = SelectGSTList(result, mainRequest, usersList);

                List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
                if (mainRequest.InvoiceType.EqualsIgnoreCase(TaxInfo.InVoice))
                {
                    listUserType = (from item in listUserType
                                    where item.UserTypeID == (int)UserType.Retailer || item.UserTypeID == (int)UserType.APIUser
                                    || item.UserTypeID == (int)UserType.Client

                                    select item).ToList();
                }

                if (mainRequest.InvoiceType.EqualsIgnoreCase(TaxInfo.SelfInvoice))
                {
                    listUserType = (from item in listUserType
                                    where item.UserTypeID == (int)UserType.Distributer || item.UserTypeID == (int)UserType.SuperDistributer
                                    select item).ToList();
                }

                List<StateMaster> statelist = new StateDAL().Select();

                list = (from main in result

                        join gst in GSTlist on main.UserID equals gst.UserID
                        join user in usersList on main.UserID equals user.UserID
                        join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                        join state in statelist on user.StateID equals state.ID
                        orderby usertype.UserTypeID, user.UserID

                        select new GSTDetailViewModel
                        {
                            GSTType = gst.GSTType,
                            ServiceCategory = main.ServiceCategory,
                            InvoiceMonth = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                            FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            UserID = user.UserID,
                            MobileNumber = user.MobileNumber,
                            GSTNo = user.GSTNo,
                            State = state.Name,
                            StateID = state.ID,
                            SuccessAmt = Math.Round(main.Qty.TwoDigitDecimalValue()),
                            SuccessCount = main.SuccessCount,
                            DiscountAmount = Math.Round(main.DiscountAmount.TwoDigitDecimalValue()),
                            CCF = Math.Round(gst.CCF.TwoDigitDecimalValue()),
                            CashBack = Math.Round(main.CashBack.TwoDigitDecimalValue()),
                            IGST = Math.Round(Math.Abs(gst.IGST.TwoDigitDecimalValue())),
                            SGST = Math.Round(Math.Abs(gst.SGST.TwoDigitDecimalValue())),
                            CGST = Math.Round(Math.Abs(gst.CGST.TwoDigitDecimalValue())),
                            TaxableValue = Math.Round(Math.Abs(gst.TaxableValue.TwoDigitDecimalValue())),
                            GSTTaxableValue = Math.Round(Math.Abs(gst.GSTTaxableValue.TwoDigitDecimalValue())),
                            UserType = usertype.UserType,
                            UserTypeID = usertype.UserTypeID,
                        }).ToList();

                list = list.Select((o, i) => { o.SequenceNumber = i + 1; return o; }).ToList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        internal List<GSTDetailViewModel> SelectGSTDetailByTransaction(GSTDetailRequest mainRequest, int UserID)
        {
            int UserTypeIDBoth = 0;
            int UserIDForSDDS = 0;

            List<GSTDetailViewModel> list = new List<GSTDetailViewModel>();
            DateTime FromDate = new DateTime(mainRequest.Year, mainRequest.Month, 1);

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            List<Project.Response.Service.ViewModel> services = new ServiceMasterDal().Select();
            List<Project.Response.Operator.ViewModel> operators = new OperatorMasterDAL().Select();
            try
            {
                List<GSTMonthlyViewModel> mainlist = SelectTransactionDatewiseWithGST(mainRequest, UserID, out UserTypeIDBoth, out UserIDForSDDS);
                List<GSTMiddleView> result = SelectGroupWiseByTrsansction(mainlist, UserTypeID, UserTypeIDBoth, UserIDForSDDS);

                List<UserMaster> usersList = new UserDAL().Select();
                List<GSTListViewModel> GSTlist = SelectGSTListByTrsansction(result, mainRequest, usersList);

                List<UserTypeMaster> listUserType = new UserTypeDAL().Select();
                if (mainRequest.InvoiceType.EqualsIgnoreCase(TaxInfo.InVoice))
                {
                    listUserType = (from item in listUserType
                                    where item.UserTypeID == (int)UserType.Retailer || item.UserTypeID == (int)UserType.APIUser
                                    || item.UserTypeID == (int)UserType.Client

                                    select item).ToList();
                }

                if (mainRequest.InvoiceType.EqualsIgnoreCase(TaxInfo.SelfInvoice))
                {
                    listUserType = (from item in listUserType
                                    where item.UserTypeID == (int)UserType.Distributer || item.UserTypeID == (int)UserType.SuperDistributer
                                    select item).ToList();
                }

                List<StateMaster> statelist = new StateDAL().Select();

                list = (from main in result
                        join gst in GSTlist on main.UserID equals gst.UserID
                        join user in usersList on main.UserID equals user.UserID
                        join usertype in listUserType on user.UserTypeID equals usertype.UserTypeID
                        join state in statelist on user.StateID equals state.ID
                        orderby usertype.UserTypeID, user.UserID

                        select new GSTDetailViewModel
                        {
                            GSTType = user.GSTPrinciple,
                            ServiceCategory = main.ServiceCategory,
                            InvoiceMonth = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                            FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                            UserID = user.UserID,
                            MobileNumber = user.MobileNumber,
                            GSTNo = user.GSTNo,
                            State = state.Name,
                            StateID = state.ID,
                            SuccessAmt = Math.Round(main.Qty.TwoDigitDecimalValue()),
                            SuccessCount = main.SuccessCount,
                            DiscountAmount = Math.Round(main.DiscountAmount.TwoDigitDecimalValue()),
                            IGST = Math.Round(Math.Abs(gst.IGST.TwoDigitDecimalValue())),
                            SGST = Math.Round(Math.Abs(gst.SGST.TwoDigitDecimalValue())),
                            CGST = Math.Round(Math.Abs(gst.CGST.TwoDigitDecimalValue())),
                            
                            TaxableValue = Math.Round(Math.Abs(((main.Qty - main.DiscountAmount) / 1.18M).TwoDigitDecimalValue())),
                            GSTTaxableValue = Math.Round(Math.Abs(main.GST.TwoDigitDecimalValue())),
                            UserType = usertype.UserType,
                            UserTypeID = usertype.UserTypeID,
                        }).ToList();

                list = list.Select((o, i) => { o.SequenceNumber = i + 1; return o; }).ToList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        internal List<GSTDetailInvoiceViewModel> SelectGSTCategoryWise(SelectByCategoryRequest mainRequest)
        {
            List<GSTDetailInvoiceViewModel> list = new List<GSTDetailInvoiceViewModel>();
            DateTime FromDate = new DateTime(mainRequest.Year, mainRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(mainRequest.UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            string GSTPrinciple = "";
            UserMaster userItem = usersList.FirstOrDefault(U => U.UserID == mainRequest.UserID);

            if (userItem != null)
            {
                GSTPrinciple = userItem.GSTPrinciple;
            }

            if (GSTPrinciple == null || GSTPrinciple.Length <= 0) GSTPrinciple = GSTTypes.Inclusion;

            int AdminstateID = (from item in usersList
                                where item.UserID == Company.AdminUserID
                                select item.StateID).FirstOrDefault();

            List<StateMaster> statelist = new StateDAL().Select();
            List<Project.Response.Service.ViewModel> services = new ServiceMasterDal().Select();
            List<Project.Response.Operator.ViewModel> operators = new OperatorMasterDAL().Select();
            try
            {
                using var context = new DatabaseContext();
                var mainTansferlist = (from txn in context.TransactionMaster
                                join service in context.ServiceMaster on txn.ServiceID equals service.ServiceID
                                where
                                txn.Status == StatusType.Success &&
                                txn.DOCInt >= From && txn.DOCInt <= To
                                 && service.ServiceCategory == mainRequest.ServiceCategory
                                && txn.OperatorID !=(int)OperatorCode.Verify_DMT
                                
                                       select new
                                {
                                    Amount = txn.Amount,
                                    DebitAmount = txn.DebitAmount,
                                    ServiceID = txn.ServiceID,
                                    ServiceCategory = service.ServiceCategory,
                                    HSNCode = txn.IsCCF ? "997159" : service.HSNCode,
                                    HSNCodeForSelfInvoice = service.HSNCodeForSelfInvoice,
                                    ServiceName = txn.IsCCF ? "REMITTANCE" : service.Name,
                                    UserID = txn.UserID,
                                    AdminGST = txn.AdminGST,
                                    RetailerCommission = txn.RetailerCommission,
                                    CashBack = txn.CashBack,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    DistributerUserID = txn.DistributerUserID,
                                    IsCCF= txn.IsCCF,
                                }).ToList();

                var mainVerifaylist = (from txn in context.TransactionMaster
                                       join service in context.ServiceMaster on txn.ServiceID equals service.ServiceID
                                       where
                                       txn.Status == StatusType.Success &&
                                       txn.DOCInt >= From && txn.DOCInt <= To
                                       && txn.OperatorID==(int)OperatorCode.Verify_DMT
                                    
                                       select new
                                       {
                                           Amount = txn.Amount,
                                           DebitAmount = txn.DebitAmount,
                                           ServiceID = txn.ServiceID,
                                           ServiceCategory = service.ServiceCategory,
                                           HSNCode = "997159",
                                           HSNCodeForSelfInvoice = service.HSNCodeForSelfInvoice,
                                           ServiceName = "VERIFICATION",
                                           UserID = txn.UserID,
                                           AdminGST = txn.AdminGST,
                                           RetailerCommission = txn.RetailerCommission,
                                           CashBack = 0m,
                                           DistributerCommission = txn.DistributerCommission,
                                           SuperDistributerCommission = txn.SuperDistributerCommission,
                                           SuperDistributerUserID = txn.SuperDistributerUserID,
                                           DistributerUserID = txn.DistributerUserID,
                                           IsCCF = txn.IsCCF,
                                       }) .ToList();

                var mainlist  = mainTansferlist.Concat(mainVerifaylist).ToList();

                if (mainRequest.UserID != 0)
                {
                    if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        mainlist = (from item in mainlist
                                    where item.SuperDistributerUserID == mainRequest.UserID
                                    select item).ToList();
                    }
                    else if (UserTypeID == (int)UserType.Distributer)
                    {
                        mainlist = (from item in mainlist
                                    where item.DistributerUserID == mainRequest.UserID
                                    select item).ToList();
                    }
                    else
                    {
                        mainlist = (from item in mainlist
                                    where item.UserID == mainRequest.UserID
                                    select item).ToList();
                    }
                }
                

                List<GSTMiddleViewInvoiceModel> resultmiddle = new List<GSTMiddleViewInvoiceModel>();

                if (UserTypeID == (int)UserType.Distributer)
                {
                    resultmiddle = (from item in mainlist

                                    group item by new { item.DistributerUserID, item.ServiceName, item.HSNCodeForSelfInvoice } into grp
                                    select new GSTMiddleViewInvoiceModel
                                    {
                                        UserID = grp.Key.DistributerUserID,
                                        ServiceName = grp.Key.ServiceName,
                                        HSNCode = grp.Key.HSNCodeForSelfInvoice,
                                        Qty = grp.Sum(tr => tr.Amount),
                                        DiscountAmount = grp.Sum(tr => tr.DistributerCommission),
                                        SuccessCount = grp.Count(),
                                        CashBack = 0m,
                                    }).ToList();
                }
                else if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    resultmiddle = (from item in mainlist

                                    group item by new { item.SuperDistributerUserID, item.ServiceName, item.HSNCodeForSelfInvoice } into grp
                                    select new GSTMiddleViewInvoiceModel
                                    {
                                        UserID = grp.Key.SuperDistributerUserID,
                                        ServiceName = grp.Key.ServiceName,
                                        HSNCode = grp.Key.HSNCodeForSelfInvoice,
                                        Qty = grp.Sum(tr => tr.Amount),
                                        DiscountAmount = grp.Sum(tr => tr.SuperDistributerCommission),
                                        SuccessCount = grp.Count(),
                                        CashBack = 0m,
                                    }).ToList();
                }
                else
                {
                    resultmiddle = (from item in mainlist

                                    group item by new { item.UserID, item.ServiceName, item.HSNCode } into grp
                                    select new GSTMiddleViewInvoiceModel
                                    {
                                        UserID = grp.Key.UserID,
                                        ServiceName = grp.Key.ServiceName,
                                        HSNCode = grp.Key.HSNCode,
                                        Qty = grp.Sum(tr => tr.Amount),
                                        DiscountAmount = grp.Sum(tr => tr.RetailerCommission),
                                        SuccessCount = grp.Count(),
                                        CashBack = grp.Sum(tr => tr.CashBack)
                                    }).ToList();
                }

                var result = (from item in resultmiddle
                              join user in usersList on item.UserID equals user.UserID
                              join state in statelist on user.StateID equals state.ID

                              select new
                              {
                                  FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                                  ServiceName = item.ServiceName,
                                  HSNCode = item.HSNCode,
                                  GSTNo = item.GSTNo,
                                  State = state.Name,
                                  UserID = item.UserID,
                                  StateID = state.ID,
                                  Qty = item.Qty,
                                  DiscountAmount = item.DiscountAmount,
                                  CashBack = item.CashBack,
                                  SuccessCount = item.SuccessCount,
                              }).ToList();

                List<GSTListViewModel> GSTlist = new List<GSTListViewModel>();

                if (TaxInfo.Prepaid.EqualsIgnoreCase(mainRequest.ServiceCategory))
                {
                    var GSTTaxable = (from main in result

                                      select new
                                      {
                                          GSTType = GSTPrinciple,
                                          ServiceName = main.ServiceName,
                                          UserID = main.UserID,
                                          StateID = main.StateID,
                                          GSTTaxableValue = ((main.Qty - main.DiscountAmount) / 1.18M) * 0.18M,
                                          TaxableValue = ((main.Qty - main.DiscountAmount) / 1.18M)
                                      }).ToList();

                    GSTlist = (from main in GSTTaxable
                               select new GSTListViewModel
                               {
                                   GSTTaxableValue = main.GSTTaxableValue,
                                   TaxableValue = main.TaxableValue,
                                   ServiceName = main.ServiceName,
                                   UserID = main.UserID,
                                   GSTType = main.GSTType,
                                   SGST = (main.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   CGST = (main.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   IGST = (main.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                               }).ToList();
                }

                if (TaxInfo.Prepaid.EqualsIgnoreCase(mainRequest.ServiceCategory)
                    && (UserTypeID == (int)UserType.Distributer || UserTypeID == (int)UserType.SuperDistributer))
                {
                    var GSTTaxable = (from main in result
                                      select new
                                      {
                                          GSTType = GSTPrinciple,
                                          ServiceName = main.ServiceName,
                                          UserID = mainRequest.UserID,
                                          StateID = main.StateID,
                                          GSTTaxableValue = (GSTPrinciple.EqualsIgnoreCase(GSTTypes.Exclusion)) ? (main.DiscountAmount * 0.18M) : (main.DiscountAmount / 1.18M) * 0.18M,
                                          TaxableValue = main.DiscountAmount,
                                      }).ToList();

                    GSTlist = (from main in GSTTaxable
                               join user in usersList on main.UserID equals user.UserID
                               select new GSTListViewModel
                               {
                                   GSTTaxableValue = main.GSTTaxableValue,
                                   TaxableValue = main.TaxableValue,
                                   GSTType = main.GSTType,
                                   UserID = main.UserID,
                                   SGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   CGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   IGST = (user.StateID != AdminstateID) ? main.GSTTaxableValue : 0
                               }).ToList();
                }

                if ((TaxInfo.DMT.EqualsIgnoreCase(mainRequest.ServiceCategory)) ||
                    (TaxInfo.Utility.EqualsIgnoreCase(mainRequest.ServiceCategory)))
                {
                   
                    var GSTTaxable = (from main in result
                                      select new
                                      {
                                          GSTType = GSTPrinciple,
                                          ServiceName = main.ServiceName,
                                          UserID = mainRequest.UserID,
                                          StateID = main.StateID,
                                          GSTTaxableValue = (GSTPrinciple.EqualsIgnoreCase(GSTTypes.Exclusion)) ? (main.DiscountAmount * 0.18M) : (main.DiscountAmount / 1.18M) * 0.18M,
                                          TaxableValue = (GSTPrinciple.EqualsIgnoreCase(GSTTypes.Exclusion)) ? (main.DiscountAmount) : (main.DiscountAmount / 1.18M),
                                          CCF= main.Qty * 0.01M
                                      }).ToList();

                    GSTlist = (from main in GSTTaxable
                               join user in usersList on main.UserID equals user.UserID
                               select new GSTListViewModel
                               {
                                   GSTTaxableValue = main.GSTTaxableValue,
                                   TaxableValue = main.TaxableValue,
                                   ServiceName = main.ServiceName,
                                   UserID = main.UserID,
                                   GSTType = main.GSTType,
                                   SGST = (main.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   CGST = (main.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   IGST = (main.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                                   CCF = main.CCF
                               }).ToList();
                }

                list = (from main in result
                        join gst in GSTlist on main.UserID equals gst.UserID
                        where gst.ServiceName == main.ServiceName
                        select new GSTDetailInvoiceViewModel
                        {
                            Transaction = main.Qty.TwoDigitDecimalValue(),
                            Discount = Math.Abs(main.DiscountAmount.TwoDigitDecimalValue()),
                            ServiceName = main.ServiceName,
                            FirmName = main.FirmName,
                            HSN = main.HSNCode,
                            GSTNo = main.GSTNo,
                            State = main.State,
                            Count = main.SuccessCount,
                            GSTType = gst.GSTType,
                            InvoiceMonth = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                            Taxable = Math.Abs(gst.TaxableValue.TwoDigitDecimalValue()),
                            IGST = Math.Abs(gst.IGST.TwoDigitDecimalValue()),
                            SGST = Math.Abs(gst.SGST.TwoDigitDecimalValue()),
                            CGST = Math.Abs(gst.CGST.TwoDigitDecimalValue()),
                            GST = Math.Abs(gst.GSTTaxableValue.TwoDigitDecimalValue()),
                            CCF = Math.Abs(gst.CCF.TwoDigitDecimalValue()),
                            CashBack = Math.Abs(main.CashBack.TwoDigitDecimalValue())
                            
                        }).Distinct().ToList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        internal List<GSTDetailInvoiceViewModel> SelectGSTCategoryWiseV2(SelectByCategoryRequest mainRequest)
        {
            List<GSTDetailInvoiceViewModel> list = new List<GSTDetailInvoiceViewModel>();
            DateTime FromDate = new DateTime(mainRequest.Year, mainRequest.Month, 1);
            DateTime LastDate = FromDate.LastOfMonth();

            int From = Convert.ToInt32(FromDate.ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(LastDate.ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(mainRequest.UserID);
            List<UserMaster> usersList = new UserDAL().Select();

            string GSTPrinciple = "";
            UserMaster userItem = usersList.FirstOrDefault(U => U.UserID == mainRequest.UserID);

            if (userItem != null)
            {
                GSTPrinciple = userItem.GSTPrinciple;
            }

            if (GSTPrinciple == null || GSTPrinciple.Length <= 0) GSTPrinciple = GSTTypes.Inclusion;

            int AdminstateID = (from item in usersList
                                where item.UserID == Company.AdminUserID
                                select item.StateID).FirstOrDefault();

            List<StateMaster> statelist = new StateDAL().Select();

            List<Project.Response.Service.ViewModel> services = new ServiceMasterDal().Select();
            List<Project.Response.Operator.ViewModel> operators = new OperatorMasterDAL().Select();
            try
            {
                using var context = new DatabaseContext();

                var mainlist = (from txn in context.TransactionMaster
                                join service in context.ServiceMaster on txn.ServiceID equals service.ServiceID
                                where
                                txn.Status == StatusType.Success &&
                                txn.DOCInt >= From && txn.DOCInt <= To
                                 && service.ServiceCategory == mainRequest.ServiceCategory
                                //     && txn.UserID == mainRequest.UserID
                                select new
                                {
                                    Amount = txn.Amount,
                                    DebitAmount = txn.DebitAmount,
                                    ServiceID = txn.ServiceID,
                                    ServiceCategory = service.ServiceCategory,
                                    HSNCode = service.HSNCode,
                                    HSNCodeForSelfInvoice = service.HSNCodeForSelfInvoice,
                                    ServiceName = service.Name,
                                    UserID = txn.UserID,
                                    AdminGST = txn.AdminGST,
                                    RetailerCommission = txn.RetailerCommission,
                                    DistributerCommission = txn.DistributerCommission,
                                    SuperDistributerCommission = txn.SuperDistributerCommission,
                                    SuperDistributerUserID = txn.SuperDistributerUserID,
                                    DistributerUserID = txn.DistributerUserID,
                                }).ToList();

                if (mainRequest.UserID != 0)
                {
                    if (UserTypeID == (int)UserType.SuperDistributer)
                    {
                        mainlist = (from item in mainlist
                                    where item.SuperDistributerUserID == mainRequest.UserID
                                    select item).ToList();
                    }
                    else if (UserTypeID == (int)UserType.Distributer)
                    {
                        mainlist = (from item in mainlist
                                    where item.DistributerUserID == mainRequest.UserID
                                    select item).ToList();
                    }
                    else
                    {
                        mainlist = (from item in mainlist
                                    where item.UserID == mainRequest.UserID
                                    select item).ToList();
                    }
                }

                List<GSTMiddleViewInvoiceModel> resultmiddle = new List<GSTMiddleViewInvoiceModel>();

                if (UserTypeID == (int)UserType.Distributer)
                {
                    resultmiddle = (from item in mainlist

                                    group item by new { item.DistributerUserID, item.ServiceName, item.HSNCodeForSelfInvoice } into grp
                                    select new GSTMiddleViewInvoiceModel
                                    {
                                        UserID = grp.Key.DistributerUserID,
                                        ServiceName = grp.Key.ServiceName,
                                        HSNCode = grp.Key.HSNCodeForSelfInvoice,
                                        Qty = grp.Sum(tr => tr.Amount),
                                        DiscountAmount = grp.Sum(tr => tr.DistributerCommission),
                                        SuccessCount = grp.Count(),
                                    }).ToList();
                }
                else if (UserTypeID == (int)UserType.SuperDistributer)
                {
                    resultmiddle = (from item in mainlist

                                    group item by new { item.SuperDistributerUserID, item.ServiceName, item.HSNCodeForSelfInvoice } into grp
                                    select new GSTMiddleViewInvoiceModel
                                    {
                                        UserID = grp.Key.SuperDistributerUserID,
                                        ServiceName = grp.Key.ServiceName,
                                        HSNCode = grp.Key.HSNCodeForSelfInvoice,
                                        Qty = grp.Sum(tr => tr.Amount),
                                        DiscountAmount = grp.Sum(tr => tr.SuperDistributerCommission),
                                        SuccessCount = grp.Count(),
                                    }).ToList();
                }
                else
                {
                    resultmiddle = (from item in mainlist

                                    group item by new { item.UserID, item.ServiceName, item.HSNCode } into grp
                                    select new GSTMiddleViewInvoiceModel
                                    {
                                        UserID = grp.Key.UserID,
                                        ServiceName = grp.Key.ServiceName,
                                        HSNCode = grp.Key.HSNCode,
                                        Qty = grp.Sum(tr => tr.Amount),
                                        DiscountAmount = grp.Sum(tr => tr.RetailerCommission),
                                        SuccessCount = grp.Count(),
                                    }).ToList();
                }

                var result = (from item in resultmiddle

                              join user in usersList on item.UserID equals user.UserID
                              join state in statelist on user.StateID equals state.ID

                              select new
                              {
                                  FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                                  ServiceName = item.ServiceName,
                                  HSNCode = item.HSNCode,
                                  GSTNo = item.GSTNo,
                                  State = state.Name,
                                  UserID = item.UserID,
                                  StateID = state.ID,
                                  Qty = item.Qty,
                                  DiscountAmount = item.DiscountAmount,
                                  SuccessCount = item.SuccessCount,
                              }).ToList();

                List<GSTListViewModel> GSTlist = new List<GSTListViewModel>();

                 if (TaxInfo.P2P.EqualsIgnoreCase(mainRequest.ServiceCategory))
                {
                    var GSTTaxable = (from main in result

                                      select new
                                      {
                                          GSTType = GSTPrinciple,
                                          ServiceName = main.ServiceName,
                                          UserID = main.UserID,
                                          StateID = main.StateID,
                                          GSTTaxableValue = ((main.Qty - main.DiscountAmount) / 1.18M) * 0.18M,
                                          TaxableValue = ((main.Qty - main.DiscountAmount) / 1.18M)
                                      }).ToList();

                    GSTlist = (from main in GSTTaxable

                               select new GSTListViewModel
                               {
                                   GSTTaxableValue = main.GSTTaxableValue,
                                   TaxableValue = main.TaxableValue,
                                   ServiceName = main.ServiceName,
                                   UserID = main.UserID,
                                   GSTType = main.GSTType,
                                   SGST = (main.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   CGST = (main.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   IGST = (main.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                               }).ToList();
                }

                if (TaxInfo.P2P.EqualsIgnoreCase(mainRequest.ServiceCategory)
                    && (UserTypeID == (int)UserType.Distributer || UserTypeID == (int)UserType.SuperDistributer))
                {
                    var GSTTaxable = (from main in result
                                      select new
                                      {
                                          GSTType = GSTPrinciple,
                                          ServiceName = main.ServiceName,
                                          UserID = mainRequest.UserID,
                                          StateID = main.StateID,
                                          GSTTaxableValue = (GSTPrinciple.EqualsIgnoreCase(GSTTypes.Exclusion)) ? (main.DiscountAmount * 0.18M) : (main.DiscountAmount / 1.18M) * 0.18M,
                                          TaxableValue = main.DiscountAmount,
                                      }).ToList();

                    GSTlist = (from main in GSTTaxable
                               join user in usersList on main.UserID equals user.UserID
                               select new GSTListViewModel
                               {
                                   GSTTaxableValue = main.GSTTaxableValue,
                                   TaxableValue = main.TaxableValue,
                                   GSTType = main.GSTType,
                                   UserID = main.UserID,
                                   SGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   CGST = (user.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   IGST = (user.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                               }).ToList();
                }

                if (TaxInfo.P2ABankingRCM.EqualsIgnoreCase(mainRequest.ServiceCategory) ||
                    TaxInfo.P2ABankingNonRCM.EqualsIgnoreCase(mainRequest.ServiceCategory) ||
                     TaxInfo.P2ANonBanking.EqualsIgnoreCase(mainRequest.ServiceCategory))

                {
                    var GSTTaxable = (from main in result
                                      select new
                                      {
                                          GSTType = GSTPrinciple,
                                          ServiceName = main.ServiceName,
                                          UserID = mainRequest.UserID,
                                          StateID = main.StateID,
                                          GSTTaxableValue = (GSTPrinciple.EqualsIgnoreCase(GSTTypes.Exclusion)) ? (main.DiscountAmount * 0.18M) : (main.DiscountAmount / 1.18M) * 0.18M,
                                          TaxableValue = (GSTPrinciple.EqualsIgnoreCase(GSTTypes.Exclusion)) ? (main.DiscountAmount) : (main.DiscountAmount / 1.18M),
                                      }).ToList();

                    GSTlist = (from main in GSTTaxable
                               join user in usersList on main.UserID equals user.UserID
                               select new GSTListViewModel
                               {
                                   GSTTaxableValue = main.GSTTaxableValue,
                                   TaxableValue = main.TaxableValue,
                                   ServiceName = main.ServiceName,
                                   UserID = main.UserID,
                                   GSTType = main.GSTType,
                                   SGST = (main.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   CGST = (main.StateID == AdminstateID) ? (main.GSTTaxableValue / 2) : 0,
                                   IGST = (main.StateID != AdminstateID) ? main.GSTTaxableValue : 0,
                               }).ToList();
                }

                list = (from main in result
                        join gst in GSTlist on main.UserID equals gst.UserID
                        where gst.ServiceName == main.ServiceName
                        select new GSTDetailInvoiceViewModel
                        {
                            Transaction = Math.Round(main.Qty.TwoDigitDecimalValue()),
                            Discount = Math.Round(Math.Abs(main.DiscountAmount.TwoDigitDecimalValue())),
                            ServiceName = main.ServiceName,
                            FirmName = main.FirmName,
                            HSN = main.HSNCode,
                            GSTNo = main.GSTNo,
                            State = main.State,
                            Count = main.SuccessCount,
                            GSTType = gst.GSTType,
                            InvoiceMonth = FromDate.ToString(Messages.OnlyMonthYearFormat, CultureInfo.InvariantCulture),
                            Taxable = Math.Round(Math.Abs(gst.TaxableValue.TwoDigitDecimalValue())),
                            IGST = Math.Round(Math.Abs(gst.IGST.TwoDigitDecimalValue())),
                            SGST = Math.Round(Math.Abs(gst.SGST.TwoDigitDecimalValue())),
                            CGST = Math.Round(Math.Abs(gst.CGST.TwoDigitDecimalValue())),
                            GST = Math.Round(Math.Abs(gst.GSTTaxableValue.TwoDigitDecimalValue())),
                        }).Distinct().ToList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }
    }
}