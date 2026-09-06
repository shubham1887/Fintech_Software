using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.AgentDetails;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Project.BAL
{
    public class AgentDetailsBAL
    {
        internal string Upload(UploadRequest mainRequest, KeyDataModel keyData, out string failureMessage, out int successfulUpdates, out int totalRecords)
        {
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                successfulUpdates = 0;
                totalRecords = 0;
                return failureMessage;
            }

            failureMessage = AgentDetailsMessages.UploadFail;
            successfulUpdates = 0;
            totalRecords = 0;

            string filename = "AgentDetails_" + Guid.NewGuid().ToString("N").Substring(0, 5) + ".xlsx";
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"Media/AgentDetails");
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

            DataTable excelUploadDT = null;
            try
            {
                excelUploadDT = new ExcelProcessing().GetDataTable(filepath);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = "Failed to read the Excel file.";
                return failureMessage;
            }
            if (excelUploadDT == null || excelUploadDT.Rows.Count <= 0)
            {
                failureMessage = "Excel file is empty or invalid.";
                return failureMessage;
            }
            totalRecords = 0;

            int batchSize = 200; // Process 500 records in each batch
            int rowCount = excelUploadDT.Rows.Count;
            for (int i = 0; i < rowCount; i += batchSize)
            {
                try
                {
                    // Extract the batch of rows
                    var batch = excelUploadDT.AsEnumerable().Skip(i).Take(batchSize).CopyToDataTable();

                    // Process the batch
                    failureMessage = ProcessBatch(batch, mainRequest.ServiceType, out int batchSuccess, out int batchTotal);
                    successfulUpdates += batchSuccess;
                    totalRecords += batchTotal;

                    if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    {
                        // If there's an error, log and continue to the next batch
                        new Logger().Write(new Exception(failureMessage));
                    }
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    failureMessage = $"Error processing batch {i / batchSize + 1}.";
                }
            }

            // Iterate over each row in the Excel file

           
            failureMessage = StatusType.Ok;
            return failureMessage;
        }


        private string ProcessBatch(DataTable batch, string serviceType, out int successfulUpdates, out int totalRecords)
        {
            successfulUpdates = 0;
            totalRecords = 0;

            if (serviceType.Equals("BBPS", StringComparison.OrdinalIgnoreCase))
            {
                foreach (DataRow row in batch.Rows)
                {
                    string agentRefId = row["Agent Reference Id"].ToString();
                    string npciAgentId = row["Agent Id"].ToString();

                    if (string.IsNullOrEmpty(agentRefId) || string.IsNullOrEmpty(npciAgentId))
                        continue;

                    totalRecords++;
                    try
                    {
                        var userMaster = new UserDAL().SelectAgentByAliceName(agentRefId);
                        if (userMaster != null)
                        {
                            userMaster.NPCIAgentID = npciAgentId;
                            new UserDAL().Update(userMaster);
                            successfulUpdates++;
                        }
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                        return "Failed to update UserMaster.";
                    }
                }
            }
            else if (serviceType.Equals("MicroATM", StringComparison.OrdinalIgnoreCase))
            {
                foreach (DataRow row in batch.Rows)
                {
                    string apiKey = row["API Key"].ToString();
                    string agentCode = row["Agent Code"].ToString();
                    string yesBankTID = row["Yes Bank TID"].ToString();
                    string merchantID = row["Merchant Id"].ToString();

                    if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(agentCode))
                        continue;

                    totalRecords++;
                    try
                    {
                        var userMaster = new UserDAL().SelectAgentByAliceName(agentCode);
                        if (userMaster != null)
                        {
                            new ProviderUserMappingBal().Insert(userMaster.UserID, Company.YesBankProviderID, "API Key", apiKey);
                            new ProviderUserMappingBal().Insert(userMaster.UserID, Company.YesBankProviderID, "Yes Bank TID", yesBankTID);

                            userMaster.PaySwiffMerchantId = merchantID;
                            new UserDAL().Update(userMaster);
                            successfulUpdates++;
                        }
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                        return "Failed to insert.";
                    }
                }
            }
            else if (serviceType.Equals("PinMaster", StringComparison.OrdinalIgnoreCase))
            {
                foreach (DataRow row in batch.Rows)
                {
                    totalRecords++;
                    try
                    {
                        var pinCode = row["PINCODE"].ToString();
                        bool isDuplicate = new Location_PinCodeMasterDal().CheckPinCode(pinCode);

                        if (!isDuplicate)
                        {
                            continue;
                        }

                        var pincodemaster = new Location_PinCodeMaster
                        {
                            PINCODE = pinCode,
                            HPY_DISTRICT = Convert.ToString(row["HPY_DISTRICT"]),
                            HYP_STATE = Convert.ToString(row["HPY_STATE"]),
                            HPY_TALUKA_CITY = Convert.ToString(row["HPY_TALUK/CITY"]),
                            REGIONNAME = Convert.ToString(row["REGIONNAME"]),
                            HPY_REGION = Convert.ToString(row["HPY_REGION"]),
                            HPY_ZONE = Convert.ToString(row["HPY_ZONE"]),
                            HPY_AREA = Convert.ToString(row["HPY_AREA"]),
                            HPY_NSM = Convert.ToString(row["HPY_NSM"]),
                            HPY_RH = Convert.ToString(row["HPY_RH"]),
                            HPY_ZSM = Convert.ToString(row["HPY_ZSM"]),
                            HPY_SH = Convert.ToString(row["HPY_SH"]),
                            HPY_CM = Convert.ToString(row["HPY_CM"]),
                            HPY_ASM = Convert.ToString(row["HPY_ASM"]),
                            HPY_FLS = Convert.ToString(row["HPY_FLS"]),
                            ACTIVATED = Convert.ToString(row["ACTIVATED"]),
                            ACTIVATION_DATE = Convert.ToString(row["ACTIVATION_DATE"]),
                            LOCATION = Convert.ToString(row["LOCATION (M/UR/SU/RU)"]),
                            WITHIN_CITY_LIMITS = Convert.ToString(row["WITHIN CITY LIMITS?"]),
                            DISTANCE_FROM_CBD = Convert.ToString(row["DISTANCE FROM CBD (KMS)"]),
                            INDIAPOST_OFFICE_NAME = Convert.ToString(row["INDIAPOST OFFICE NAME"]),
                            DIVISIONNAME = Convert.ToString(row["DIVISIONNAME"]),
                            LONGITUDE = Convert.ToString(row["LONGITUDE"]),
                            LATITUDE = Convert.ToString(row["LATITUDE"]),
                            EMPLOYEE_CODE = Convert.ToString(row["EMPLOYEE_CODE"]),
                            EMPLOYEE_NAME = Convert.ToString(row["EMPLOYEE_NAME"]),
                            EMPLOYEE_MOBILE = Convert.ToString(row["EMPLOYEE_MOBILE"]),
                            EMPLOYEE_EMAIL = Convert.ToString(row["EMPLOYEE_EMAIL"]),
                            RESERVEFIELD_5 = Convert.ToString(row["RESERVE FIELD_5"]),
                            RESERVEFIELD_6 = Convert.ToString(row["RESERVE FIELD_6"]),
                            RESERVEFIELD_7 = Convert.ToString(row["RESERVE FIELD_7"]),
                            RESERVEFIELD_8 = Convert.ToString(row["RESERVE FIELD_8"]),
                            RESERVEFIELD_9 = Convert.ToString(row["RESERVE FIELD_9"]),
                            RESERVEFIELD_10 = Convert.ToString(row["RESERVE FIELD_10"]),
                            RESERVEFIELD_11 = Convert.ToString(row["RESERVE FIELD_11"]),
                            RESERVEFIELD_12 = Convert.ToString(row["RESERVE FIELD_12"]),
                            RESERVEFIELD_13 = Convert.ToString(row["RESERVE FIELD_13"]),
                            RESERVEFIELD_14 = Convert.ToString(row["RESERVE FIELD_14"]),
                            RESERVEFIELD_15 = Convert.ToString(row["RESERVE FIELD_15"]),

                            IsActive = true,
                            IsDelete = false
                        };

                        new Location_PinCodeMasterDal().Insert(pincodemaster);
                        successfulUpdates++;
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                        return $"Failed to process record with PINCODE: {row["PINCODE"]}.";
                    }
                }
            }

            return StatusType.Ok;
        }



        internal Response.Agent.UserWrapper SelectByPincode(SelectPincodeRequest mainRequest, out string failureMessage)
        {
            Response.Agent.UserWrapper wrapper = new Response.Agent.UserWrapper();
            failureMessage = "";
            Location_PinCodeMaster locationPinCode = new Location_PinCodeMaster();
            try
            {
                locationPinCode = new Location_PinCodeMasterDal().SelectByPincode(mainRequest.PinCode);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = "An error occurred while fetching data.";
                return wrapper;
            }

            if (locationPinCode == null)
            {
                failureMessage = UserMessage.DataNotFound;
                return wrapper;
            }

            var list = new List<Location_PinCodeMaster> { locationPinCode };

            wrapper.Count = list.Count();
            
            wrapper.list = list; 
            failureMessage = StatusType.Ok; 
            return wrapper;
        }

    }
}