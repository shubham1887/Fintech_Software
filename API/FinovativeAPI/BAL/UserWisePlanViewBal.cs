using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.UserWisePlanView;
using Project.Response.UserWisePlanView;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class UserWisePlanViewBal
    {
        internal string Insert(int OperatorID,string ConsumerNumber,int UserID)
        {

            int serviceID = new OperatorMasterDAL().SelectServiceID(OperatorID);

            UserWisePlanView UserWisePlan = new UserWisePlanView
            {
                UserID = UserID,
                OperatorID = OperatorID,
                ConsumerNumber = ConsumerNumber,
                ServiceID = serviceID,
            };

            try
            {
                int rowsAfftected = new UserWisePlanViewDal().Insert(UserWisePlan);

                if (rowsAfftected <= 0)
                {
                    return UserCreationMessage.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;

        }

        internal ReportWrapper Select(SelectRequest mainRequest, int UserID, out string failureMessage)
        {
            List<ViewModel> response = null;
            ReportWrapper wrapper = new ReportWrapper();
            isExcelOrPDF(mainRequest);
            try
            {
                response = new UserWisePlanViewDal().SelectReport(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (response.Count <= 0 || response == null)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }

            response = response.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            response = response.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();
            wrapper.Count = response.Count;

            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }

            if (!mainRequest.IsExcelDownload)
            {
                response = response.AsQueryable().SkipAndTakeCount(mainRequest.SkipCount, mainRequest.TakeCount).ToList();
            }


            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = response;
            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(SelectRequest request)
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
