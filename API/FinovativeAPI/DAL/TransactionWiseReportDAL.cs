using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Project.BAL;
using Project.Model;
using Project.Processing;
using Project.Response.TransactionWiseReport;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class TransactionWiseReportDAL
    {
        internal List<OpratorWiseDateWiseViewModel> SelectOperatorWisesUserWiseReport(Project.Requests.TransactionWiseReport.TransactionWiseReportRequest mainRequest, int UserID)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<OpratorWiseDateWiseViewModel> userRangeCounts;

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                userRangeCounts = context.TransactionMaster
                    .Where(transactionMaster =>
                     (GetRole(UserTypeID) == (int)UserType.SuperDistributer ? transactionMaster.SuperDistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Distributer ? transactionMaster.DistributerUserID == UserID :
                                       GetRole(UserTypeID) == (int)UserType.Admin ? 2 > 1 : transactionMaster.UserID == UserID)
                                       && (GetSelectedUserRole(UserTypeID, mainRequest.UserID) == (int)UserType.SuperDistributer ? transactionMaster.SuperDistributerUserID == mainRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, mainRequest.UserID) == (int)UserType.Distributer ? transactionMaster.DistributerUserID == mainRequest.UserID :
                                       GetSelectedUserRole(UserTypeID, mainRequest.UserID) == (int)UserType.Admin ? 2 > 1 :
                                       GetSelectedUserRole(UserTypeID, mainRequest.UserID) == -1 ? 2 > 1 : transactionMaster.UserID == mainRequest.UserID) &&
                       transactionMaster.DOCInt >= FromDate &&
                        transactionMaster.DOCInt <= ToDate &&
                        transactionMaster.Status == StatusType.Success)
                    .OrderByDescending(transactionMaster => transactionMaster.TransactionMasterID)
                    .AsEnumerable()
                    .GroupBy(transactionMaster => new
                    {
                        DOC = transactionMaster.DOC.ShortFormatDate()
                    })
            .Select(group => new OpratorWiseDateWiseViewModel
            {
                DOC = group.Key.DOC,
                MobileSaleID = (int)ServiceCode.Mobile,
                MobileSale = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.Mobile).Sum(txn => txn.Amount),
                DTHSaleID = (int)ServiceCode.Dth,
                DTHSale = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.Dth).Sum(txn => txn.Amount),
                MoneyTransferID = (int)ServiceCode.MoneyTransfer,
                MoneyTransfer = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.MoneyTransfer).Sum(txn => txn.Amount),
                PaymentGatewaySaleID = (int)ServiceCode.PaymentGateway,
                PaymentGatewaySale = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.PaymentGateway).Sum(txn => txn.Amount),
                AepsSaleID = (int)ServiceCode.AEPS,
                AepsSale = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.AEPS).Sum(txn => txn.Amount),
                ElectricityID = (int)ServiceCode.Electricity,
                Electricity = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.Electricity).Sum(txn => txn.Amount),
                PostPaidID = (int)ServiceCode.PostPaid,
                PostPaid = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.PostPaid).Sum(txn => txn.Amount),
                MAtmID = (int)ServiceCode.MATM,
                MAtm = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.MATM).Sum(txn => txn.Amount),
                GeneralInsuranceID = (int)ServiceCode.GeneralInsurance,
                GeneralInsurance = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.GeneralInsurance).Sum(txn => txn.Amount),
                BroadbandID = (int)ServiceCode.Broadband,
                Broadband = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.Broadband).Sum(txn => txn.Amount),
                GasCylinderBookingID = (int)ServiceCode.GasCylinderBooking,
                GasCylinderBooking = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.GasCylinderBooking).Sum(txn => txn.Amount),
                PipedGasID = (int)ServiceCode.PipedGas,
                PipedGas = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.PipedGas).Sum(txn => txn.Amount),
                PayoutID = (int)ServiceCode.Payout,
                Payout = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.Payout).Sum(txn => txn.Amount),
                UPITransferID = (int)ServiceCode.UPITransfer,
                UPITransfer = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.UPITransfer).Sum(txn => txn.Amount),
                POSID = (int)ServiceCode.POS,
                POS = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.POS).Sum(txn => txn.Amount),
                ExpressMoneyTransferID = (int)ServiceCode.ExpressMoneyTransfer,
                ExpressMoneyTransfer = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.ExpressMoneyTransfer).Sum(txn => txn.Amount),
                WaterBillID = (int)ServiceCode.Waterbill,
                WaterBill = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.Waterbill).Sum(txn => txn.Amount),
                FasTagID = (int)ServiceCode.Fasttag,
                FasTag = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.Fasttag).Sum(txn => txn.Amount),
                CreditCardID = (int)ServiceCode.CreditCardPayment,
                CreditCard = group.Where(transaction => transaction.ServiceID == (int)ServiceCode.CreditCardPayment).Sum(txn => txn.Amount),
                CMSID = (int)ServiceCode.CMS,
                CMS = group.Where(transaction => transaction.ServiceID != (int)ServiceCode.CMS).Sum(txn => txn.Amount),
                TotalService = group.Sum(txn => txn.Amount)
            })
                    .ToList();
            }
            return userRangeCounts;
        }

        internal List<UserWiseOpratorWiseViewModel> SelectUserWiseOperatorWiseSaleReport(Project.Requests.TransactionWiseReport.TransactionWiseReportRequest mainRequest, int UserID)
        {
            int TodayDate = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            int PastDate = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).AddDays(-1).ToString(Messages.DOCIntegerFomrat));

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            List<UserWiseOpratorWiseViewModel> userRangeCounts;
            List<UserMaster> UserMaster = new UserDAL().Select();
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var query = context.TransactionMaster.AsQueryable();
                query = query.Where(transactionMaster =>
                    (transactionMaster.DOCInt == TodayDate || transactionMaster.DOCInt == PastDate) &&
                    transactionMaster.Status == StatusType.Success);

                if (mainRequest.UserTypeID == (int)UserType.SuperDistributer)
                {
                    query = query.Where(transactionMaster =>transactionMaster.SuperDistributerUserID > 0 )
                       .Select(txn => new TransactionMaster
                       {
                           UserID = txn.SuperDistributerUserID,
                           ServiceID = txn.ServiceID,
                           Amount = txn.Amount,
                           DOCInt = txn.DOCInt
                       });
                    if (mainRequest.UserID > 0)
                    {
                        query = query.Where(txn => txn.UserID == mainRequest.UserID);
                    }
                }

                else if (mainRequest.UserTypeID == (int)UserType.Distributer)
                {
                    query = query.Where(transactionMaster => transactionMaster.DistributerUserID > 0)
                           .Select(txn => new TransactionMaster
                           {
                               UserID = txn.DistributerUserID,
                               ServiceID = txn.ServiceID,
                               Amount = txn.Amount,
                               DOCInt = txn.DOCInt
                           });
                    if (mainRequest.UserID > 0)
                    {
                        query = query.Where(txn => txn.UserID == mainRequest.UserID);
                    }
                }

                else
                {
                    query = query.Where(transactionMaster => mainRequest.UserID == 0 ? 2 > 1 : transactionMaster.UserID == mainRequest.UserID).Select(txn => new TransactionMaster
                    {
                        UserID = txn.UserID,
                        ServiceID = txn.ServiceID,
                        Amount = txn.Amount,
                        DOCInt = txn.DOCInt
                    });

                }

                var transactions = query
                    .Select(txn => new
                    {
                        txn.UserID,
                        txn.ServiceID,
                        txn.Amount,
                        txn.DOCInt
                    })
                    .ToList();

                var UserMasterQuery = from userMaster in context.UserMaster
                                      where mainRequest.UserTypeID > 0 ? userMaster.UserTypeID == mainRequest.UserTypeID : userMaster.UserTypeID > 0
                                      select new
                                      {
                                          UserID = userMaster.UserID,
                                          UserTypeID = userMaster.UserType.UserType,
                                          FirmName = userMaster.FirmName,
                                      };

                var userMasters = UserMasterQuery.ToList();

                var joinedQuery = from userMaster in userMasters
                                  join transaction in transactions
                                  on userMaster.UserID equals transaction.UserID
                                  select new
                                  {
                                      userMaster.UserID,
                                      userMaster.FirmName,
                                      userMaster.UserTypeID,
                                      transaction.ServiceID,
                                      transaction.Amount,
                                      IsToday = transaction.DOCInt == TodayDate
                                  };

                userRangeCounts = joinedQuery
                    .GroupBy(transaction => new
                    {
                        transaction.UserID,
                        transaction.FirmName,
                        transaction.UserTypeID,
                    })
                    .Select(group => new UserWiseOpratorWiseViewModel
                    {
                        UserID = group.Key.UserID,
                        FirmName = group.Key.FirmName,
                        Role = group.Key.UserTypeID,
                        MobileSaleID = (int)ServiceCode.Mobile,
                        TodayMobileSale = group.Where(txn => txn.ServiceID == (int)ServiceCode.Mobile && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayMobileSale = group.Where(txn => txn.ServiceID == (int)ServiceCode.Mobile && !txn.IsToday).Sum(txn => txn.Amount),
                        DTHSaleID = (int)ServiceCode.Dth,
                        TodayDTHSale = group.Where(txn => txn.ServiceID == (int)ServiceCode.Dth && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayDTHSale = group.Where(txn => txn.ServiceID == (int)ServiceCode.Dth && !txn.IsToday).Sum(txn => txn.Amount),
                        MoneyTransferID = (int)ServiceCode.MoneyTransfer,
                        TodayMoneyTransfer = group.Where(txn => txn.ServiceID == (int)ServiceCode.MoneyTransfer && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayMoneyTransfer = group.Where(txn => txn.ServiceID == (int)ServiceCode.MoneyTransfer && !txn.IsToday).Sum(txn => txn.Amount),
                        PaymentGatewaySaleID = (int)ServiceCode.PaymentGateway,
                        TodayPaymentGatewaySale = group.Where(txn => txn.ServiceID == (int)ServiceCode.PaymentGateway && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayPaymentGatewaySale = group.Where(txn => txn.ServiceID == (int)ServiceCode.PaymentGateway && !txn.IsToday).Sum(txn => txn.Amount),
                        AepsSaleID = (int)ServiceCode.AEPS,
                        TodayAepsSale = group.Where(txn => txn.ServiceID == (int)ServiceCode.AEPS && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayAepsSale = group.Where(txn => txn.ServiceID == (int)ServiceCode.AEPS && !txn.IsToday).Sum(txn => txn.Amount),
                        ElectricityID = (int)ServiceCode.Electricity,
                        TodayElectricity = group.Where(txn => txn.ServiceID == (int)ServiceCode.Electricity && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayElectricity = group.Where(txn => txn.ServiceID == (int)ServiceCode.Electricity && !txn.IsToday).Sum(txn => txn.Amount),
                        PostPaidID = (int)ServiceCode.PostPaid,
                        TodayPostPaid = group.Where(txn => txn.ServiceID == (int)ServiceCode.PostPaid && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayPostPaid = group.Where(txn => txn.ServiceID == (int)ServiceCode.PostPaid && !txn.IsToday).Sum(txn => txn.Amount),
                        MAtmID = (int)ServiceCode.MATM,
                        TodayMAtm = group.Where(txn => txn.ServiceID == (int)ServiceCode.MATM && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayMAtm = group.Where(txn => txn.ServiceID == (int)ServiceCode.MATM && !txn.IsToday).Sum(txn => txn.Amount),
                        GeneralInsuranceID = (int)ServiceCode.GeneralInsurance,
                        TodayGeneralInsurance = group.Where(txn => txn.ServiceID == (int)ServiceCode.GeneralInsurance && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayGeneralInsurance = group.Where(txn => txn.ServiceID == (int)ServiceCode.GeneralInsurance && !txn.IsToday).Sum(txn => txn.Amount),
                        BroadbandID = (int)ServiceCode.Broadband,
                        TodayBroadband = group.Where(txn => txn.ServiceID == (int)ServiceCode.Broadband && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayBroadband = group.Where(txn => txn.ServiceID == (int)ServiceCode.Broadband && !txn.IsToday).Sum(txn => txn.Amount),
                        PipedGasID = (int)ServiceCode.PipedGas,
                        TodayPipedGas = group.Where(txn => txn.ServiceID == (int)ServiceCode.PipedGas && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayPipedGas = group.Where(txn => txn.ServiceID == (int)ServiceCode.PipedGas && !txn.IsToday).Sum(txn => txn.Amount),
                        PayoutID = (int)ServiceCode.Payout,
                        TodayPayout = group.Where(txn => txn.ServiceID == (int)ServiceCode.Payout && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayPayout = group.Where(txn => txn.ServiceID == (int)ServiceCode.Payout && !txn.IsToday).Sum(txn => txn.Amount),
                        UPITransferID = (int)ServiceCode.UPITransfer,
                        TodayUPITransfer = group.Where(txn => txn.ServiceID == (int)ServiceCode.UPITransfer && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayUPITransfer = group.Where(txn => txn.ServiceID == (int)ServiceCode.UPITransfer && !txn.IsToday).Sum(txn => txn.Amount),
                        POSID = (int)ServiceCode.POS,
                        TodayPOS = group.Where(txn => txn.ServiceID == (int)ServiceCode.POS && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayPOS = group.Where(txn => txn.ServiceID == (int)ServiceCode.POS && !txn.IsToday).Sum(txn => txn.Amount),
                        ExpressMoneyTransferID = (int)ServiceCode.ExpressMoneyTransfer,
                        TodayExpressMoneyTransfer = group.Where(txn => txn.ServiceID == (int)ServiceCode.ExpressMoneyTransfer && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayExpressMoneyTransfer = group.Where(txn => txn.ServiceID == (int)ServiceCode.ExpressMoneyTransfer && !txn.IsToday).Sum(txn => txn.Amount),
                        FasTagID = (int)ServiceCode.Fasttag,
                        TodayFasTag = group.Where(txn => txn.ServiceID == (int)ServiceCode.Fasttag && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayFasTag = group.Where(txn => txn.ServiceID == (int)ServiceCode.Fasttag && !txn.IsToday).Sum(txn => txn.Amount),
                        CreditCardID = (int)ServiceCode.CreditCardPayment,
                        TodayCreditCard = group.Where(txn => txn.ServiceID == (int)ServiceCode.CreditCardPayment && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayCreditCard = group.Where(txn => txn.ServiceID == (int)ServiceCode.CreditCardPayment && !txn.IsToday).Sum(txn => txn.Amount),
                        CMSID = (int)ServiceCode.CMS,
                        TodayCMS = group.Where(txn => txn.ServiceID == (int)ServiceCode.CMS && txn.IsToday).Sum(txn => txn.Amount),
                        YesterdayCMS = group.Where(txn => txn.ServiceID == (int)ServiceCode.CMS && !txn.IsToday).Sum(txn => txn.Amount),
                        TodayTotalService = group.Where(txn => txn.IsToday).Sum(txn => txn.Amount),
                        PastTotalService = group.Where(txn => !txn.IsToday).Sum(txn => txn.Amount)
                    })
                    .ToList();

                return userRangeCounts;
            }
        }

        private int GetRole(int UserTypeID)
        {
            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                return (int)UserType.Admin;
            }
            return UserTypeID;
        }

        private int GetSelectedUserRole(int UserTypeID, int selectedUserID)
        {
            if (selectedUserID == 0)
            {
                return -1;
            }
            if (Util.isRoleAdminOrSupport(UserTypeID) && selectedUserID == 0)
            {
                return (int)UserType.Admin;
            }
            int RequestedUserIDUserTypeID = new UserDAL().SelectUserTypeID(selectedUserID);

            return RequestedUserIDUserTypeID;
        }
    }
}
