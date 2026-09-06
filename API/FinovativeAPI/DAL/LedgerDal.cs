using Project.Model;
using Project.Processing;
using Project.Requests.Ledger;
using Project.Response.Ledger;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class LedgerDal
    {
        internal List<LedgerResponse> Select(SelectRequest selectRequest, int LoginUserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<LedgerResponse> list = new List<LedgerResponse>();
            using var context = new DatabaseContext();
            list = (from transactionDetails in context.TransactionDetails
                    join userMaster in context.UserMaster on
                    transactionDetails.UserID equals userMaster.UserID
                    join assignuser in context.UserMaster on
                       transactionDetails.AssignedUserID equals assignuser.UserID
                    join wallet in context.WalletMaster on transactionDetails.WalletID equals wallet.WalletID
                    join service in context.ServiceMaster on transactionDetails.ServiceID equals service.ServiceID
                    join traMaster in context.TransactionMaster on transactionDetails.ReferenceID equals traMaster.TransactionMasterID
                   into traMasterdetails
                    from LtraM in traMasterdetails.DefaultIfEmpty()
                    where transactionDetails.DOCInt >= From
                    && transactionDetails.DOCInt <= To
                    && transactionDetails.DOCType != DOCType.CASH_COLLECTION
                    // && transactionDetails.UserID == selectRequest.UserID

                    && (selectRequest.WalletID == 0 ? 2 > 1 : transactionDetails.WalletID == selectRequest.WalletID)
            && (
                selectRequest.Criteria.EqualsIgnoreCase("ConsumerNumber") ? transactionDetails.ConsumerNumber == selectRequest.CriteriaText :
               selectRequest.Criteria.EqualsIgnoreCase("ReferenceID") ? transactionDetails.ReferenceID == int.Parse(selectRequest.CriteriaText) : 2 > 1)

                    orderby transactionDetails.TransactionDetailID descending

                    select new LedgerResponse
                    {
                        TransactionDetailID = transactionDetails.TransactionDetailID,
                        UserID = transactionDetails.UserID,
                        FirmName = userMaster.FirmName.ConcatNameMobileAliasName(userMaster.MobileNumber, userMaster.AliasName),
                        AssignedUserName = assignuser.FirmName.ConcatNameMobileAliasName(assignuser.MobileNumber, assignuser.AliasName),
                        OpeningBalance = transactionDetails.OpeningBalance,
                        ConsumerNumber = LtraM.ConsumerNumber,
                        Amount = transactionDetails.TransactionAmount,
                        WalletID = transactionDetails.WalletID,
                        TDS =transactionDetails.TDS,
                        WalletName = wallet.Name,
                        ReferenceID = transactionDetails.ReferenceID.ToString(),
                        TransactionID = LtraM.ReferenceNo,
                        DOCType = transactionDetails.DOCType,
                        ClosingBalance = transactionDetails.ClosingBalance,
                        CommissionAmount = transactionDetails.DiscountAmount,
                        Description = transactionDetails.TransactionDescription,
                        DOC = transactionDetails.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                        AmountCreditDebit = transactionDetails.AmountCreditDebit,
                        customAmount = new CustomAmount
                        {
                            Amount = transactionDetails.TransactionAmount,
                            Status = transactionDetails.AmountCreditDebit
                        },
                        ServiceName = service.Name,
                        RechargeDate = LtraM.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                    }).ToList();
            if (selectRequest.Criteria.EqualsIgnoreCase("TransactionID"))
            {
                list = list.Where(txn => txn.TransactionID == selectRequest.CriteriaText).ToList();
            }
            int LoginUserTypeID = new UserDAL().SelectUserTypeID(LoginUserID);

            if (LoginUserTypeID == (int)UserType.Retailer || LoginUserTypeID == (int)UserType.APIUser)
                selectRequest.UserID = LoginUserID;

            if (selectRequest.UserID > 0)
            {
                list = (from item in list
                        where item.UserID == selectRequest.UserID
                        select item).ToList();
            }

            if (selectRequest.UserID < 0)
            {
                List<Response.User.DownlineForDropDownResponse> downlinelist = null;

                downlinelist = new UserDAL().SelectDownlineForLedger(LoginUserID, LoginUserTypeID);

                if (downlinelist == null || downlinelist.Count <= 0) return list;

                if (selectRequest.UserTypeID > 0)
                {
                    downlinelist = (from item in downlinelist
                                    where item.UserTypeID == selectRequest.UserTypeID
                                    select item).ToList();
                }

                //  if (downlinelist.Count <= 0) return list;

                list = (from item in list
                        join downline in downlinelist on
                        item.UserID equals downline.UserID
                        select item).ToList();
            }

            return list;
        }

        internal int SelectTransactionCount(SelectRequest selectRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<TransactionDetails> list = new List<TransactionDetails>();
            using var context = new DatabaseContext();
            list = (from transactionDetails in context.TransactionDetails
                    where transactionDetails.DOCInt >= From
                   && transactionDetails.DOCInt <= To
                   && transactionDetails.DOCType != DOCType.CASH_COLLECTION
                   && transactionDetails.UserID == selectRequest.UserID
                    && (selectRequest.WalletID == 0 ? 2 > 1 : transactionDetails.WalletID == selectRequest.WalletID)
                    select transactionDetails).ToList();
            return list.Count();
        }

        internal List<LedgerHeader> SelectHeaderData(SelectRequest selectRequest)
        {
            int currentDateInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            int DayaddTo = Convert.ToInt32((Convert.ToDateTime(selectRequest.ToDate).AddDays(1)).ToString(Messages.DOCIntegerFomrat));

            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));
            int UserTypeID = new UserDAL().SelectUserTypeID(selectRequest.UserID);
            DateTime selectedFromDate = Convert.ToDateTime(selectRequest.FromDate);
            DateTime selectedToDate = Convert.ToDateTime(selectRequest.ToDate);

            using var context = new DatabaseContext();

            var aepsFund = (from transactionDetails in context.TransactionMaster
                            join userMaster in context.UserMaster on
                            transactionDetails.UserID equals userMaster.UserID
                            where transactionDetails.DOCInt >= From
                            && transactionDetails.DOCInt <= To
                            && transactionDetails.ServiceID == (int)ServiceCode.AEPS
                            && transactionDetails.Status == StatusType.Success
                            orderby transactionDetails.TransactionMasterID descending

                            select new
                            {
                                UserID = transactionDetails.UserID,
                                AepsAmount = transactionDetails.Amount,
                            }).ToList();
            if (selectRequest.UserID != Company.AdminUserID)
            {
                aepsFund = (from item in aepsFund
                            where item.UserID == selectRequest.UserID
                            select item).ToList();
            }

            var matmFund = (from transactionDetails in context.TransactionMaster
                            join userMaster in context.UserMaster on
                            transactionDetails.UserID equals userMaster.UserID
                            where transactionDetails.DOCInt >= From
                            && transactionDetails.DOCInt <= To
                            && transactionDetails.ServiceID == (int)ServiceCode.MATM
                            && transactionDetails.UserID == selectRequest.UserID
                            && transactionDetails.Status == StatusType.Success
                            orderby transactionDetails.TransactionMasterID descending

                            select new
                            {
                                UserID = transactionDetails.UserID,
                                MATMAmount = transactionDetails.Amount,
                            }).ToList();

            var pgFund = (from transactionDetails in context.PaymentGateway
                          join userMaster in context.UserMaster on
                          transactionDetails.UserID equals userMaster.UserID
                          where transactionDetails.DOCInt >= From
                          && transactionDetails.DOCInt <= To
                          && transactionDetails.UserID == selectRequest.UserID
                          && transactionDetails.Status == StatusType.Success
                          orderby transactionDetails.PaymentGatewayID descending

                          select new
                          {
                              UserID = transactionDetails.UserID,
                              pgRetailerCommission = transactionDetails.Commission,
                              pgDistributerCommission = transactionDetails.DistributerCommission,
                              pgSuperDistributerCommission = transactionDetails.SuperDistributerCommission,
                          }).ToList();

            var mainFund = (from fund in context.FundTransaction
                            where (fund.DOCInt >= From
                            && fund.DOCInt <= To)
                            || (fund.UpdatedDOC.Date >= selectedFromDate.Date
                            && fund.UpdatedDOC.Date <= selectedToDate.Date)
                            && (fund.SenderID == selectRequest.UserID || fund.ReceiverID == selectRequest.UserID)

                            select new
                            {
                                Amount = fund.Amount,
                                SenderID = fund.SenderID,
                                WalletID = fund.WalletID,
                                ReceiverID = fund.ReceiverID,
                                Through = fund.Through,
                                UserID = selectRequest.UserID
                            }).ToList();

            if (selectRequest.WalletID != 0)
            {
                mainFund = (from item in mainFund
                            where item.WalletID == selectRequest.WalletID
                            select item).ToList();
            }

            var FundGroupList = (from fund in mainFund
                                 group fund by new { fund.UserID } into grp
                                 select new
                                 {
                                     TransferAmount = grp.Where(q => q.SenderID == selectRequest.UserID
                                        && (q.Through == FundTransferType.ThroughRequestTransfer
                                        || q.Through == FundTransferType.ThroughDirectTransfer)).Sum(X => X.Amount),

                                     PGAmount = grp.Where(q => q.SenderID == selectRequest.UserID
                                        && (q.Through == FundTransferType.PGTransfer)).Sum(X => X.Amount),

                                     ECollectionAmount = grp.Where(q => q.SenderID == selectRequest.UserID
                                  && (q.Through == FundTransferType.ECollection)).Sum(X => X.Amount),

                                     ReceiverAmount = grp.Where(q => q.ReceiverID == selectRequest.UserID
                                       && (q.Through == FundTransferType.ThroughDirectTransfer
                                           || q.Through == FundTransferType.ThroughRequestTransfer)).Sum(X => X?.Amount ?? 0),

                                     ReverseAmountDr = grp.Where(q => q.SenderID == selectRequest.UserID
                                        && (q.Through == FundTransferType.ThroughDirectReverse
                                    || q.Through == FundTransferType.ThroughReverseRequestTransfer)).Sum(X => X.Amount),

                                     ReverseAmountCr = grp.Where(q => q.ReceiverID == selectRequest.UserID
                                       && (q.Through == FundTransferType.ThroughDirectReverse
                                   || q.Through == FundTransferType.ThroughReverseRequestTransfer)).Sum(X => X.Amount),
                                 }).FirstOrDefault();

            var FundReciveGroupList = (from fund in mainFund
                                       group fund by new { fund.UserID } into grp
                                       select new
                                       {
                                           PGAmount = grp.Where(q => q.ReceiverID == selectRequest.UserID
                                              && (q.Through == FundTransferType.PGTransfer)).Sum(X => X.Amount),
                                           ECollectionAmount = grp.Where(q => q.ReceiverID == selectRequest.UserID
                                        && (q.Through == FundTransferType.ECollection)).Sum(X => X.Amount),
                                       }).FirstOrDefault();

            var AEPSFundGroupList = (from fund in aepsFund
                                     group fund by 1 into grp
                                     select new
                                     {
                                         AepsAmount = grp.Sum(X => X.AepsAmount),
                                     }).FirstOrDefault();

            var MATMFundGroupList = (from fund in matmFund
                                     group fund by new { fund.UserID } into grp
                                     select new
                                     {
                                         MATMAmount = grp.Sum(X => X.MATMAmount),
                                     }).FirstOrDefault();

            decimal virtualBal = 0;
            if (selectRequest.WalletID != 0)
                virtualBal = new VirtualBalanceDal().SelectVirtualBalance(selectRequest.WalletID, From, To);

            SucessAmountResponse ObjSucess;
            if (selectRequest.WalletID != 0)
            {
                ObjSucess = new TransactionDAL().SelectSucessAmountByWallet(selectRequest.UserID, From, To, selectRequest.WalletID);
            }
            else
            {
                ObjSucess = new TransactionDAL().SelectSucessAllAmount(selectRequest.UserID, From, To);
            }

            decimal Commission = 0;
            decimal Surcharge = 0;
            decimal AEPSCommission = 0;
            decimal TDS = 0;
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                Commission = new TransactionDAL().SelectSuperDistributerCommi(selectRequest.UserID, From, To, selectRequest.WalletID);
                Surcharge = new TransactionDAL().SelectSuperDistributerSurcharge(selectRequest.UserID, From, To, selectRequest.WalletID);
                TDS = new TransactionDAL().SelectSuperDistributerTDS(selectRequest.UserID, From, To, selectRequest.WalletID);
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                Commission = new TransactionDAL().SelectDistributerCommi(selectRequest.UserID, From, To, selectRequest.WalletID);
                Surcharge = new TransactionDAL().SelectDistributerSurcharge(selectRequest.UserID, From, To, selectRequest.WalletID);
                TDS = new TransactionDAL().SelectDistributerTDS(selectRequest.UserID, From, To, selectRequest.WalletID);
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                Commission = new TransactionDAL().SelectRetailerCommi(selectRequest.UserID, From, To, selectRequest.WalletID);
                Surcharge = new TransactionDAL().SelectRetailerSurcharge(selectRequest.UserID, From, To, selectRequest.WalletID);
                TDS = new TransactionDAL().SelectRetailerTDS(selectRequest.UserID, From, To, selectRequest.WalletID);
            }
            if (UserTypeID == (int)UserType.Admin)
            {
                Commission = new TransactionDAL().SelectTotalCommi(From, To, selectRequest.WalletID);
                Surcharge = new TransactionDAL().SelectTotalSurcharge(From, To, selectRequest.WalletID);
                AEPSCommission = new TransactionDAL().SelectAEPSCommi(From, To);
            }

            var PGGroupList = (from fund in pgFund
                               group fund by new { fund.UserID } into grp
                               select new
                               {
                                   pgRetailerCommission = grp.Sum(X => X.pgRetailerCommission),
                                   pgDistributerCommission = grp.Sum(X => X.pgDistributerCommission),
                                   pgSuperDistributerCommission = grp.Sum(X => X.pgSuperDistributerCommission),
                               }).FirstOrDefault();

            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                Commission = Commission + (PGGroupList?.pgSuperDistributerCommission ?? 0);
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                Commission = Commission + (PGGroupList?.pgDistributerCommission ?? 0);
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                Commission = Commission + (PGGroupList?.pgRetailerCommission ?? 0);
            }
            if (UserTypeID == (int)UserType.Admin)
            {
                Commission = Commission + (PGGroupList?.pgRetailerCommission ?? 0) + (PGGroupList?.pgDistributerCommission ?? 0) + (PGGroupList?.pgSuperDistributerCommission ?? 0);
            }

            PastResponse pastResponse = null;
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                pastResponse = new TransactionDAL().SelectPastSucessFailAmount(selectRequest.UserID, Convert.ToDateTime(selectRequest.FromDate), Convert.ToDateTime(selectRequest.ToDate), selectRequest.WalletID);
            }
            decimal opBal = 0;
            opBal = new UserBalanceHistoryDAL().SelectOpeningBalanceByWalletID(selectRequest.UserID, From, selectRequest.WalletID);

            decimal TotalComplaintAmount = new ComplaintMasterDal().SelectTotalAmount(From, To, selectRequest.UserID);
            decimal ClosingBalance = 0;

            if (currentDateInt == To)
            {
                if (selectRequest.WalletID != 0 && selectRequest.UserID > 0)
                {
                    UserWalletMaster current = new UserWalletMasterDal().SelectByWalletIDAndUserID(new UserWalletMaster
                    {
                        UserID = selectRequest.UserID,
                        WalletID = selectRequest.WalletID,
                    });
                    ClosingBalance = current.Balance;
                }
                else
                {
                    ClosingBalance = new UserWalletMasterDal().SelectSumByUserID(selectRequest.UserID);
                }
            }
            else
            {
                if (selectRequest.WalletID != 0)
                {
                    ClosingBalance = new UserBalanceHistoryDAL().SelectClosingBalance(selectRequest.UserID, selectRequest.WalletID, DayaddTo);
                }
                else
                {
                    ClosingBalance = new UserBalanceHistoryDAL().SelectAllClosingBalance(selectRequest.UserID, DayaddTo);
                }
            }

            LedgerHeader ledgerHeader = new LedgerHeader();
            ledgerHeader.ComplaintAmount = TotalComplaintAmount;
            ledgerHeader.OpeningBal = opBal;
            ledgerHeader.TransferAmount = FundGroupList?.TransferAmount ?? 0;
            if (UserTypeID == (int)UserType.Admin)
            {
                ledgerHeader.PGAmount = FundGroupList?.PGAmount ?? 0;
                ledgerHeader.ECollectionAmount = FundGroupList?.ECollectionAmount ?? 0;
            }
            else
            {
                ledgerHeader.PGAmount = FundReciveGroupList?.PGAmount ?? 0;
                ledgerHeader.ECollectionAmount = FundReciveGroupList?.ECollectionAmount ?? 0;
            }

            ledgerHeader.AEPSAmount = AEPSFundGroupList?.AepsAmount ?? 0;
            ledgerHeader.MATMAmount = MATMFundGroupList?.MATMAmount ?? 0;
            ledgerHeader.ReverseAmountDr = FundGroupList?.ReverseAmountDr ?? 0;
            ledgerHeader.ReverseAmountCr = FundGroupList?.ReverseAmountCr ?? 0;
            ledgerHeader.Requested = ObjSucess?.Request ?? 0;
            ledgerHeader.Debited = ObjSucess?.Debited ?? 0;
            ledgerHeader.ClosingBalance = ClosingBalance;
            ledgerHeader.PastSuccessAmount = pastResponse?.PastSuccessAmount ?? 0;
            ledgerHeader.PastFailAmount = pastResponse?.PastFailAmount ?? 0;

            if (UserTypeID == (int)UserType.Admin)
            {
                ledgerHeader.ReceiverAmount = virtualBal;
            }
            else
            {
                ledgerHeader.ReceiverAmount = FundGroupList?.ReceiverAmount ?? 0;
            }
            ledgerHeader.CommissionAmount = Commission;
            ledgerHeader.TDS = TDS;
            ledgerHeader.Surcharge = Surcharge;

            decimal TotalCreditAmount;
            decimal TotalDebitAmount;
            if (UserTypeID == (int)UserType.Admin)
            {
                TotalCreditAmount = (ledgerHeader.OpeningBal + ledgerHeader.ReverseAmountCr + ledgerHeader.ReceiverAmount + ledgerHeader.PastFailAmount);
                TotalDebitAmount = (ledgerHeader.Requested + ledgerHeader.TransferAmount + ledgerHeader.PastSuccessAmount + ledgerHeader.PGAmount + ledgerHeader.ECollectionAmount + ledgerHeader.AEPSAmount + ledgerHeader.MATMAmount + AEPSCommission);
            }
            else
            {
                TotalCreditAmount = (ledgerHeader.OpeningBal + ledgerHeader.ReverseAmountCr + ledgerHeader.CommissionAmount + ledgerHeader.ReceiverAmount + ledgerHeader.PastFailAmount + ledgerHeader.PGAmount + ledgerHeader.ECollectionAmount + ledgerHeader.AEPSAmount + ledgerHeader.MATMAmount);
                TotalDebitAmount = (ledgerHeader.Requested + ledgerHeader.Surcharge + ledgerHeader.TransferAmount + ledgerHeader.PastSuccessAmount + ledgerHeader.TDS);
            }
            decimal ExpectedCloseBal = TotalCreditAmount - TotalDebitAmount;
            decimal Difference = ClosingBalance - ExpectedCloseBal;
            ledgerHeader.ExpectedCloseBal = ExpectedCloseBal;
            ledgerHeader.Difference = Difference;

            List<LedgerHeader> Headerlist = new List<LedgerHeader>();
            Headerlist.Add(ledgerHeader);

            return Headerlist;
        }

        internal List<AdminLedgerViewModel> AdminLedger(AdminRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<AdminLedgerViewModel> list = new List<AdminLedgerViewModel>();
            using (var context = new DatabaseContext())
            {
                var MainDebit = (from fund in context.FundTransaction
                                 where fund.DOCInt >= From
                                 && fund.DOCInt <= To
                                 && fund.SenderID == UserID
                                 && fund.Status == StatusType.Success

                                 select new
                                 {
                                     DOC = fund.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                                     Description = fund.Description ?? "",
                                     FundTransactionID = fund.FundTransactionID,
                                     Remark = fund.AdminRemark,
                                     Debit = fund.Amount,
                                     Credit = 0.0M,
                                     OpeningBalance = fund.SenderOpeningBalance,
                                     ClosingBalance = fund.SenderClosingBalance,
                                     WalletID = fund.WalletID,
                                     BankName = fund.BankName,
                                     Through = fund.Through
                                 }).ToList();

                var MainCredit = (from fund in context.FundTransaction
                                  where fund.DOCInt >= From
                                  && fund.DOCInt <= To
                                  && fund.ReceiverID == UserID
                                  && fund.Status == StatusType.Success
                                  select new
                                  {
                                      DOC = fund.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                                      Description = fund.Description ?? "",
                                      FundTransactionID = fund.FundTransactionID,
                                      Remark = fund.AdminRemark,
                                      Debit = 0.0M,
                                      Credit = fund.Amount,
                                      OpeningBalance = fund.ReceiverOpeningBalance,
                                      ClosingBalance = fund.ReceiverClosingBalance,
                                      WalletID = fund.WalletID,
                                      BankName = fund.BankName,
                                      Through = fund.Through
                                  }).ToList();

                var listFinal = MainDebit.Union(MainCredit).ToList();

                List<WalletMaster> walletslist = new WalletMasterDal().Select();

                list = (from item in listFinal
                        join wallet in walletslist on item.WalletID equals wallet.WalletID
                        orderby item.FundTransactionID descending
                        select new AdminLedgerViewModel
                        {
                            DOC = item.DOC,
                            WalletName = wallet.Name,
                            Description = item.Description,
                            Remark = item.Remark,
                            DebitAmount = item.Debit,
                            CreditAmount = item.Credit,
                            OpeningBalance = item.OpeningBalance,
                            ClosingBalance = item.ClosingBalance,
                            WalletID = item.WalletID,
                            BankName = item.BankName,
                            Through = item.Through
                        }).ToList();

                if (selectRequest.WalletID != 0)
                {
                    list = (from item in list
                            where item.WalletID == selectRequest.WalletID
                            select item).ToList();
                }
            }
            return list;
        }
    }
}