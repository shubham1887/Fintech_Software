using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.SignUp;
using Project.Response.FailedSignUpData;
using Project.Response.SignUp;
using Project.StaticModel;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Project.BAL
{
    public class FailedSignUpDataBal
    {
        internal FailedSignUpDataResponse Insert(SignUpVerifyRequest request, out string failureMessage)
        {
            FailedSignUpDataResponse response = new FailedSignUpDataResponse();
            failureMessage = request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            FailedSignUpData model = new FailedSignUpData
            {
                MobileNumber = request.MobileNumber,
                PANNo = request.PANNo,
                PinCode = request.PinCode,
                ReferralCode = request?.ReferralCode ?? "",
                EmployeeID = request?.EmployeeID ?? "",
                Status = StatusType.Process,
            };
            int rowsAfftected = new FailedSignUpDataDal().Insert(model);
            if (rowsAfftected <= 0)
            {
                failureMessage = SignUpMessage.InsertFailure;
                return response;
            }
            response.Status = StatusType.Success;
            response.Message = SignUpMessage.CustomerInquiry;
            failureMessage = SignUpMessage.CustomerInquiry;
            return response;
        }

        internal Response.SignUp.FailedUserWrapper SelectReport(SelectReportRequest mainRequest, out string failureMessage)
        {
            Response.SignUp.FailedUserWrapper wrapper = new Response.SignUp.FailedUserWrapper();
            failureMessage = "";
            List<SelectFailSignUpResponse> list = new List<SelectFailSignUpResponse>();

            try
            {
                list = new FailedSignUpDataDal().SelectReport(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }


            failureMessage = UserMessage.DataNotFound;
            if (list.Count <= 0) return wrapper;

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            wrapper.Count = list.Count();
            if (!mainRequest.IsExcelDownload)
            {
                list = list.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;
            failureMessage = StatusType.Ok;
            return wrapper;
        }
    }
}