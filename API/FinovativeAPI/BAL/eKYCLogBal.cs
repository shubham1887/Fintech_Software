using Newtonsoft.Json;
using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.eKYCLog;
using Project.Response.eKYCLog;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class eKYCLogBal
    {
        internal void Insert(int UserID, string DocumentNo, string DocumentType, string Responsejson)
        {
            eKYCLog ObjModel = new eKYCLog
            {
                UserID = UserID,
                DocumentNo = DocumentNo,
                DocumentType = DocumentType,
                Response = Responsejson
            };
            try
            {
                int rowsAfftected = new eKYCLogDal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
        }

        internal ViewModel SelectByUserID(string DocumentType, int UserID)
        {
            ViewModel viewModel = null;
            try
            {
                viewModel = new eKYCLogDal().SelectByUserID(DocumentType, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }

        internal Response.eKYC.eKycReportWrapper Select(SelectRequest mainRequest, int UserID)
        {
            Response.eKYC.eKycReportWrapper wrapper = new Response.eKYC.eKycReportWrapper();
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            List<Response.eKYC.EkycDataViewModel> actualData = new List<Response.eKYC.EkycDataViewModel>();
            try
            {
                List<Response.eKYC.ViewModel> report = new eKYCLogDal().Select(mainRequest, UserID);
                foreach (var item in report)
                {

                    Response.eKYC.EkycDataViewModel ekycData = JsonConvert.DeserializeObject<Response.eKYC.EkycDataViewModel>(item.Response);
                    ekycData.FirmName = item.FirmName;
                    ekycData.UserID = item.UserID;
                    ekycData.EmailID = item.EmailID;
                    ekycData.PANNo = item.PANNo;
                    ekycData.DocumentType = item.DocumentType;
                    ekycData.DocumentNo = item.DocumentNo;
                    ekycData.MobileNumber = item.MobileNumber;
                    actualData.Add(ekycData);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return wrapper;
            }
            wrapper.Count = actualData.Count;
            actualData = actualData.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            if (!mainRequest.IsExcelDownload)
            {
                actualData = actualData.AsQueryable().SkipAndTakeCount(mainRequest.SkipCount, mainRequest.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = actualData;
            return wrapper;
        }

        internal void Delete(int userID)
        {
            try
            {
                new eKYCLogDal().Delete(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}