using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;

using Project.StaticModel;
using System;
using Microsoft.AspNetCore.Cors;

[CustomAuthorization]
//[Authorize]
[Route("[controller]/[action]")]
[EnableCors("CORSProject")]
[ApiController]
public class TransactionWiseReportController : ControllerBase
{
    private readonly EncryptDecrypt ed = new EncryptDecrypt();

    [HttpPost]
    public Project.Response.APIResponse SelectOperatorWisesUserWiseReport([FromForm] Project.Requests.APIRequest aPIRequest)
    {
        Project.Requests.KeyDataModel keyDataModel;
        Project.Requests.TransactionWiseReport.TransactionWiseReportRequest mainRequest;
        try
        {
            keyDataModel = JsonConvert.DeserializeObject<Project.Requests.KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            mainRequest = JsonConvert.DeserializeObject<Project.Requests.TransactionWiseReport.TransactionWiseReportRequest>(ed.Decrypt(aPIRequest.Request));
        }
        catch (Exception ex)
        {
            new Logger().Write(ex);
            return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
        }
        Project.Response.TransactionWiseReport.OpratorWiseDateWiseWrapper wrapper = new Project.Response.TransactionWiseReport.OpratorWiseDateWiseWrapper();
        string failureMessage;

        if (keyDataModel == null || mainRequest == null) return ResponseProcess.GetFailReason(Messages.InValidRequest);

        try
        {
            wrapper = new TransactionWiseReportBAL().SelectOperatorWisesUserWiseReport(mainRequest, keyDataModel, out failureMessage);
        }
        catch (Exception ex)
        {
            new Logger().Write(ex);
            return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
        }

        if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            return ResponseProcess.GetFailReason(failureMessage);

        return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
    }
    [HttpPost]
    public Project.Response.APIResponse SelectUserWiseOperatorWiseSellReport([FromForm] Project.Requests.APIRequest aPIRequest)
    {
        Project.Requests.KeyDataModel keyDataModel;
        Project.Requests.TransactionWiseReport.TransactionWiseReportRequest mainRequest;
        try
        {
            keyDataModel = JsonConvert.DeserializeObject<Project.Requests.KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            mainRequest = JsonConvert.DeserializeObject<Project.Requests.TransactionWiseReport.TransactionWiseReportRequest>(ed.Decrypt(aPIRequest.Request));
        }
        catch (Exception ex)
        {
            new Logger().Write(ex);
            return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
        }
        Project.Response.TransactionWiseReport.UserWiseOpratorWiseWrapper wrapper = new Project.Response.TransactionWiseReport.UserWiseOpratorWiseWrapper();
        string failureMessage;

        if (keyDataModel == null || mainRequest == null) return ResponseProcess.GetFailReason(Messages.InValidRequest);

        try
        {
            wrapper = new TransactionWiseReportBAL().SelectUserWiseOperatorWiseSaleReport(mainRequest, keyDataModel, out failureMessage);
        }
        catch (Exception ex)
        {
            new Logger().Write(ex);
            return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
        }

        if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            return ResponseProcess.GetFailReason(failureMessage);

        return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wrapper));
    }

}

