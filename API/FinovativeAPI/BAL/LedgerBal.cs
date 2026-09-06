using Project.DAL;
using Project.Processing;
using Project.Requests.Ledger;
using Project.Response.Ledger;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class LedgerBal
    {
        internal LedgerViewModel Select(SelectRequest selectRequest, int LoginUserID, out string failureMessage)
        {
            List<LedgerResponse> report = new List<LedgerResponse>();
            LedgerViewModel response = new LedgerViewModel();

            failureMessage = selectRequest.IsValidModalState();

            isExcelOrPDF(selectRequest);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            failureMessage = Messages.DataNotFound;
            try
            {
                report = new LedgerDal().Select(selectRequest, LoginUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            response.Count = report.Count();

            if (!Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
            {
                report = report.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();
            }
            report = report.AsQueryable().WhereDynamic(selectRequest.SearchText).ToList();
            if (report == null) return response;

            foreach (LedgerResponse item in report)
            {
                LedgerResponse model = new LedgerResponse
                {
                    TransactionDetailID = item.TransactionDetailID,
                    UserID = item.UserID,
                    FirmName = item.FirmName,
                    AssignedUserName = item.AssignedUserName,
                    OpeningBalance = item.OpeningBalance,
                    Amount = item.Amount,
                    TDS = item.TDS,
                    WalletID = item.WalletID,
                    WalletName = item.WalletName,
                    ConsumerNumber = item.ConsumerNumber,
                    ReferenceID = item.ReferenceID,
                    TransactionID = item.TransactionID,
                    RechargeDate = item.RechargeDate,
                    DOCType = item.DOCType,
                    ClosingBalance = item.ClosingBalance,
                    CommissionAmount = item.CommissionAmount,
                    Description = item.Description,
                    DOC = item.DOC,
                    customAmount = item.customAmount,
                    AmountCreditDebit = item.AmountCreditDebit,
                    ServiceName = item.ServiceName,
                };

                if (item.DOCType == DOCType.Recharge)
                {
                    model.ReferenceID = new TransactionDAL().SelectReferenceNoByTransactionMasterID(Convert.ToInt32(item.ReferenceID));
                }
                if (item.DOCType == DOCType.FundTrnasfer || item.DOCType == DOCType.FundReceive || item.DOCType == DOCType.FundReverse)
                {
                    model.ReferenceID = new FundTransactionDal().SelectReferenceNoByFundTransactionID(Convert.ToInt32(item.ReferenceID));
                    model.Remark = new FundTransactionDal().SelectRemarkByFundTransactionID(Convert.ToInt32(item.ReferenceID));
                }

                response.list.Add(model);
            }
            
            if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
            {
                response.IsExcelDownload = true;
            }

            if (selectRequest.SkipCount <= 0)
            {
                response.Headerlist = new LedgerDal().SelectHeaderData(selectRequest);
                // response.Count = new LedgerDal().SelectTransactionCount(selectRequest);
            }
            failureMessage = StatusType.Ok;
            return response;
        }

        internal AdminLedgerWrapper AdminLedger(AdminRequest selectRequest, int UserID, out string failureMessage)
        {
            AdminLedgerWrapper response = new AdminLedgerWrapper();
            List<AdminLedgerViewModel> report = new List<AdminLedgerViewModel>();
            try
            {
                report = new LedgerDal().AdminLedger(selectRequest, UserID);
                if (report == null || report.Count <= 0)
                {
                    failureMessage = Messages.DataNotFound;
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            response.data = report;
            response.IsExcelDownload = selectRequest.IsExcelDownload;
            failureMessage = StatusType.Ok;
            return response;
        }


        internal static bool isExcelOrPDF(Requests.Ledger.SelectRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }

            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
    }
}