using Project.DAL;
using Project.Message;
using Project.Processing;
using Project.Requests;
using Project.Requests.Reconciliation;
using Project.Requests.Transaction;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Project.BAL
{
    public class ReconciliationBal
    {
        internal List<Project.Response.Reconciliation.ViewModel> Upload(UploadRequest mainRequest, KeyDataModel keyData, out string failureMessage)
        {
            List<Project.Response.Reconciliation.ViewModel> report = new List<Response.Reconciliation.ViewModel>();
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return report;
            failureMessage = "Please add valid settings for provider";

            Project.Response.ReconciliationSettings.ViewModel setting = null;
            try
            {
                setting = new ReconciliationSettingBal().SelectByProviderID(mainRequest.ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return report;
            }
            if (setting == null)
                return report;

            failureMessage = ReconciliationMessages.UploadFail;

            string filename = "Reconciliation_" + Guid.NewGuid().ToString("N").Substring(0, 5) + ".xlsx";
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"Media/ReconcillationFiles");

            string filepath = Path.Combine(path, filename);

            try
            {
                DirectoryInfo di = Directory.CreateDirectory(path);

                byte[] fileinbytes = Convert.FromBase64String(mainRequest.ExcelDocument.Data);
                File.WriteAllBytes(filepath, fileinbytes);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            DataTable excelUploadDT = null;
            try
            {
                excelUploadDT = new ExcelProcessing().GetDataTable(filepath);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (excelUploadDT == null || excelUploadDT.Rows.Count <= 0) return report;

            List<Response.TransactionMaster.ViewModel> dbList = new List<Response.TransactionMaster.ViewModel>();

            try
            {
                dbList = new TransactionDAL().SelectForFilter(new ReportRequest
                {
                    FromDate = mainRequest.FromDate,
                    ToDate = mainRequest.ToDate,
                    ProviderID = mainRequest.ProviderID,
                    Status = "ALL",
                    TakeCount = 100000,
                    SkipCount = 0
                }, keyData.UserID);
            }
            catch (Exception ex)
            {
                return report;
            }
            if (dbList == null || dbList.Count <= 0)
            {
                failureMessage = "Transactions not found";
                return report;
            }

            DataTable dbDT = null;
            try
            {
                dbDT = ExcelProcessing.ConvertListToDataTable(dbList);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return report;
            }
            if (dbDT == null || dbDT.Rows.Count <= 0) return report;

            decimal APITotalSuccessAmount = GetTotalAmountByStatus(excelUploadDT, setting.Amount, setting.Status, "SUCCESS");
            decimal APITotalFailureAmount = GetTotalAmountByStatus(excelUploadDT, setting.Amount, setting.Status, "Fail");
            decimal APITotalProcessAmount = GetTotalAmountByStatus(excelUploadDT, setting.Amount, setting.Status, "Process");

            decimal OwnTotalSuccessAmount = GetTotalAmountByStatus(dbDT, setting.Amount, setting.Status, StatusType.Success);
            decimal OwnTotalFailureAmount = GetTotalAmountByStatus(dbDT, setting.Amount, setting.Status, StatusType.Failure);
            decimal OwnTotalProcessAmount = GetTotalAmountByStatus(dbDT, setting.Amount, setting.Status, StatusType.Process);

            if (APITotalSuccessAmount != OwnTotalSuccessAmount ||
                    APITotalFailureAmount != OwnTotalFailureAmount ||
                    APITotalProcessAmount != OwnTotalProcessAmount)
            {
                report = Compare(dbDT, excelUploadDT, setting.APIUserRequestID, setting.ConsumerNumber, setting.Status, setting.Amount, setting.SuccessStatusAs, setting.FailureStatusAs);
            }

            failureMessage = StatusType.Ok;

            return report;
        }

        private List<Project.Response.Reconciliation.ViewModel> Compare(DataTable dbDataTable, DataTable excelUploadDataTable,
            string RequestID, string Number, string Status, string Amount, string SuccessStatusAs, string FailureStatusAs)
        {
            dbDataTable.DefaultView.Sort = "ReferenceNo";
            dbDataTable = dbDataTable.DefaultView.ToTable();

            excelUploadDataTable.DefaultView.Sort = RequestID;
            excelUploadDataTable = excelUploadDataTable.DefaultView.ToTable();
            List<Project.Response.Reconciliation.ViewModel> report = new List<Response.Reconciliation.ViewModel>();

            Project.Response.Reconciliation.ViewModel ObjModel = null;
            foreach (DataRow excelRow in excelUploadDataTable.Rows)
            {
                string APIConsumerNumber = excelRow[Number].ToString();
                string apiRefNo = excelRow[RequestID].ToString();
                string apiStatus = excelRow[Status].ToString();
                string apiAmount = excelRow[Amount].ToString();

                // check if apiStatus exists in SuccessStatusAs and FailureStatusAs

                string apialternateStatus = GetMatchableStatusfromExcelStatus(apiStatus, SuccessStatusAs, FailureStatusAs);

                apiStatus = apialternateStatus;

                if (apiRefNo == "") continue;
                ObjModel = new Response.Reconciliation.ViewModel();

                ObjModel.Number = APIConsumerNumber;
                ObjModel.ExcelStatus = apiStatus;
                ObjModel.IsMismached = false;

                foreach (DataRow dbRow in dbDataTable.Rows)
                {
                    string dbRefNo = dbRow["ReferenceNo"].ToString();
                    string dbStatus = dbRow["Status"].ToString();
                    string dbAmount = Convert.ToInt32(dbRow["Amount"]).ToString();

                    string DBConsumerNumber = excelRow[Number].ToString();
                    if (apiRefNo.EqualsIgnoreCase(dbRefNo))
                    {
                        if (APIConsumerNumber.EqualsIgnoreCase(DBConsumerNumber))
                        {
                            if (apiAmount.EqualsIgnoreCase(dbAmount))
                            {
                                ObjModel.FirmName = dbRow["FirmName"].ToString();
                                ObjModel.Operator = dbRow["OperatorName"].ToString();
                                ObjModel.DOC = dbRow["DOC"].ToString();
                                ObjModel.DBStatus = dbStatus;
                                ObjModel.Amount = apiAmount;

                                if (!apiStatus.EqualsIgnoreCase(dbStatus))
                                {
                                    ObjModel.IsMismached = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                report.Add(ObjModel);
            }
            return report;
        }

        private string GetMatchableStatusfromExcelStatus(string apiStatus, string successStatusAs, string failureStatusAs)
        {
            if (apiStatus == null || apiStatus.Length <= 0) return StatusType.Process;

            if (apiStatus.EqualsIgnoreCase(successStatusAs)) return StatusType.Success;

            if (apiStatus.EqualsIgnoreCase(failureStatusAs)) return StatusType.Failure;

            string[] successSplit = successStatusAs.Split(',');

            foreach (string item in successSplit)
            {
                if (item.EqualsIgnoreCase(apiStatus))
                {
                    return StatusType.Success;
                }
            }

            string[] failSplit = failureStatusAs.Split(',');

            foreach (string item in failSplit)
            {
                if (item.EqualsIgnoreCase(apiStatus))
                {
                    return StatusType.Failure;
                }
            }

            return apiStatus;
        }

        private decimal GetTotalAmountByStatus(DataTable dt, string amountcolumnName, string StatusColumnName, string status)
        {
            decimal total = 0;
            try

            {
                total = dt.AsEnumerable().Where(row => row.Field<string>(StatusColumnName) == status).Sum(row => row.Field<decimal>(amountcolumnName));
            }
            catch (Exception ex)
            {
                return total;
            }

            return total;
        }
    }
}