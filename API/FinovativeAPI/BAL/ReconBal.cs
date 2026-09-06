using CsvHelper;
using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.AgentDetails;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;

namespace Porject.BAL
{
    public class ReconBal
    {
        internal string Upload(UploadRequest mainRequest, out string failureMessage, out int successfulUpdates, out int totalRecords)
        {
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                successfulUpdates = 0;
                totalRecords = 0;
                return failureMessage;
            }

            failureMessage = ReconciliationMessages.UploadFail;
            successfulUpdates = 0;
            totalRecords = 0;

            string extension = Path.GetExtension(mainRequest.ExcelDocument.Name).ToLower();
            string filename = "ReconFile_" + Guid.NewGuid().ToString("N").Substring(0, 5) + extension;
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"Media/ReconFile");
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
                failureMessage = "File could not be saved.";
                return failureMessage;
            }

            DataTable fileData = null;
            try
            {
                if (extension == ".xlsx")
                {
                    fileData = new ExcelProcessing().GetDataTable(filepath);
                }
                else if (extension == ".csv")
                {
                    fileData = GetDataTableFromCsv(filepath);
                }
                else
                {
                    failureMessage = "Unsupported file format. Please upload .xlsx or .csv files.";
                    return failureMessage;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = "Failed to read the file.";
                return failureMessage;
            }

            if (fileData == null || fileData.Rows.Count <= 0)
            {
                failureMessage = "The file is empty or invalid.";
                return failureMessage;
            }

            List<string> errorMessages = new List<string>();
            List<DataRow> validRows = new List<DataRow>();

            totalRecords = fileData.Rows.Count;

            for (int i = 0; i < fileData.Rows.Count; i++)
            {
                DataRow row = fileData.Rows[i];
                int rowNumber = i + 1;

                string validationError = ValidateRow(row, rowNumber);
                if (validationError != null)
                    errorMessages.Add(validationError);
                else
                {
                    validRows.Add(row);
                }
            }

            if (errorMessages.Count > 0)
            {
                failureMessage = string.Join(Environment.NewLine, errorMessages);
                return failureMessage;
            }

            foreach (DataRow row in validRows)
            {
                string requestID = row["RequestID"].ToString().Trim();
                string status = row["Status"].ToString().Trim();
                string referanceNo = row["ReferanceNo"].ToString().Trim();
                string date = row["Date"].ToString().Trim();


                if (status.EqualsIgnoreCase(StatusType.Success))
                {
                    TransactionMaster transactionMaster = new TransactionDAL().SelectByProviderRequestIDAndDate(requestID, int.Parse(date));
                    if (transactionMaster == null)
                        continue;

                    string faileureMessage = isValidForBulkSuccessUpdate(transactionMaster);
                    if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                        continue;

                    if (!isValidBulkUpdateSuccessByServiceID(transactionMaster))
                        return null;
                    transactionMaster.Status = StatusType.Success;
                    transactionMaster.ReferenceNo = referanceNo;

                    transactionMaster.Description = TransactionMessage.TransSucessDescription;
                    new AccountCoreProcessing().SuccessTransaction(transactionMaster);
                }
                if (status.EqualsIgnoreCase(StatusType.Failure))
                {
                    TransactionMaster transactionMaster = new TransactionDAL().SelectByProviderRequestIDAndDate(requestID, int.Parse(date));
                    if (transactionMaster == null)
                        continue;

                    string docType = GetDocType(transactionMaster.WalletID);
                    string faileureMessage = isValidForBulkUpdate(transactionMaster);

                    if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                        continue;

                    if (!isValidBulkUpdateFailByServiceID(transactionMaster))
                        return null;

                    transactionMaster.Status = StatusType.Failure;
                    transactionMaster.ReferenceNo = referanceNo;

                    transactionMaster.Description = TransactionMessage.TransFailDescription;
                    new AccountCoreProcessing().RefundTransaction(transactionMaster, transactionMaster.Description, docType);
                }

                successfulUpdates++;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        private DataTable GetDataTableFromCsv(string filepath)
        {
            DataTable csvDataTable = new DataTable();
            using (var reader = new StreamReader(filepath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                using (var dr = new CsvDataReader(csv))
                {
                    csvDataTable.Load(dr);
                }
            }
            return csvDataTable;
        }

        private string isValidForBulkUpdate(TransactionMaster transactionMaster)
        {
            if (new ResponseProcess().CheckFailure(transactionMaster.Status))
                return TransactionMessage.AllreadyFail;

            return StatusType.Ok;
        }
        private bool isValidBulkUpdateFailByServiceID(TransactionMaster transactionMaster)
        {
            if (transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer)
                return false;

            return true;
        }
        private bool isValidBulkUpdateSuccessByServiceID(TransactionMaster transactionMaster)
        {
            if (transactionMaster.ServiceID == (int)ServiceCode.MATM)
                return false;
            if (transactionMaster.ServiceID == (int)ServiceCode.AEPS)
                return false;
            return true;
        }


        private string isValidForBulkSuccessUpdate(TransactionMaster transactionMaster)
        {
            if (!new ResponseProcess().checkProcess(transactionMaster.Status))
                return TransactionMessage.NotInProcess;

            return StatusType.Ok;
        }

        private string GetDocType(int walletID)
        {
            WalletMaster wallet = new WalletBal().SelectByID(walletID);
            return wallet.Name;
        }


        private string ValidateRow(DataRow row, int rowNumber)
        {

            string date = row["Date"].ToString().Trim();
            string requestID = row["RequestID"].ToString().Trim();
            string amount = row["Amount"].ToString().Trim();
            string status = row["Status"].ToString().Trim();
            string referanceNo = row["ReferanceNo"].ToString().Trim();

            if (string.IsNullOrWhiteSpace(date) ||
                string.IsNullOrWhiteSpace(requestID) ||
                string.IsNullOrWhiteSpace(amount) ||
                string.IsNullOrWhiteSpace(status))
            {
                return $"Validation failed: One or more required fields are missing in row {rowNumber}.";
            }

            if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return $"Validation failed: 'Date' must be in the 'yyyymmdd' format in row {rowNumber}.";
            }

            if (requestID.Length == 0)
            {
                return $"Validation failed: 'RequestID' must be valid in row {rowNumber}.";
            }

            if (requestID.Length > 0)
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByProviderRequestIDAndDate(requestID, int.Parse(date));
                if (transactionMaster == null)
                    return $"Validation failed: 'RequestID' must be valid in row {rowNumber}.";
            }

            if (!decimal.TryParse(amount, out decimal parsedAmount))
            {
                return $"Validation failed: 'Amount' must be a valid number in row {rowNumber}.";
            }

            var allowedStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                StatusType.Success,
                StatusType.Failure,
            };

            if (!allowedStatuses.Contains(status))
            {
                return $"Validation failed: 'Status' in row {rowNumber} is not one of the allowed values.";
            }

            if (status.EqualsIgnoreCase(StatusType.Success))
            {
                if (referanceNo == null || referanceNo.Length <= 5)

                    return $"Validation failed: For Success in row {rowNumber} must contain valid referance number.";
            }

            return null;
        }
    }
}
