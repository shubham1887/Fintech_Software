using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.CashCollection;
using Project.Response.CashCollection;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class CashCollectionDetailBal
    {
        internal int Insert(int UserID, int ParentID, decimal OutstandingOpening, decimal OutstandingAmount, decimal OutstandingClosing, string ParentRemark
            , string PayMode, string Credit, string Debit, int WalletID, string UTRNumber, string Type, string ToDate)
        {
            using (ConcurrencyControl.AcquireLock(UserID, WalletID.ToString()))
            {
                return InsertCore(UserID, ParentID, OutstandingOpening, OutstandingAmount, OutstandingClosing, ParentRemark, PayMode, Credit, Debit, UTRNumber, Type, ToDate);
            }
        }

        private int InsertCore(int UserID, int ParentID, decimal OutstandingOpening, decimal OutstandingAmount, decimal OutstandingClosing, string ParentRemark, string PayMode, string Credit, string Debit, string UTRNumber, string Type, string ToDate)
        {
            CashCollectionDetail ObjModel = new CashCollectionDetail
            {
                UserID = UserID,
                ParentID = ParentID,
                OutstandingOpening = OutstandingOpening,
                OutstandingAmount = OutstandingAmount,
                OutstandingClosing = OutstandingClosing,
                ParentRemark = ParentRemark,
                PayMode = PayMode,
                Credit = Credit,
                Debit = Debit,
                UTRNumber = UTRNumber,
                Type = Type
            };
            if (IsValidForBackDate(ToDate))
            {
                int TransferDate = Convert.ToInt32(Convert.ToDateTime(ToDate).ToString(Messages.DOCIntegerFomrat));
                int currentDate = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));

                if (TransferDate != currentDate)
                {
                    DateTime dt = new Util().ParseRequestDate(ToDate);
                    DateTime DateWithtime = dt.Add(DateTime.Now.TimeOfDay);
                    ObjModel.DOC = (Convert.ToDateTime(DateWithtime));
                    ObjModel.DOCInt = Convert.ToInt32(dt.ToString(Messages.DOCIntegerFomrat));
                }
            }
            int result = 0;
            try
            {
                result = new CashCollectionDetailDal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return 0;
            }
            return result;
        }

        private bool IsValidForBackDate(string ToDate)
        {
            if (!Company.BackDateFundTransfer.EqualsIgnoreCase("Yes"))
                return false;

            if (ToDate == null)
                return false;

            if (ToDate.Length <= 0)
                return false;

            return true;
        }

        internal CollectionReportWrapper SelectByFilter(ReportRequest mainRequest, int CurrentUserID, out string failureMessage)
        {
            CollectionReportWrapper response = new CollectionReportWrapper();
            failureMessage = mainRequest.IsValidModalState();
            isExcelOrPDF(mainRequest);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            List<ViewModel> views = new List<ViewModel>();
            views = new CashCollectionDetailDal().SelectByFilter(mainRequest, CurrentUserID);

            if (views.Count == 0)
            {
                failureMessage = Messages.DataNotFound;
                return response;
            }
            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                response.IsDownload = true;
            }
            response.data = views;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal static bool isExcelOrPDF(Requests.CashCollection.ReportRequest request)
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