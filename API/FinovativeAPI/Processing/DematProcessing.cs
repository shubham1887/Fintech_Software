using Project.Model;
using Project.Requests.Demat;
using Project.StaticModel;
using System;

namespace Project.Processing
{
    public class DematProcessing
    {
        public string SendMail(InitRequest mainRequest, DematSetting dematSetting, string requestID)
        {
            string Subject = $"New Lead for {mainRequest.PersonName}-{mainRequest.MobileNumber}-{mainRequest.EmailID}";
            string message = $@"
<html>
    <body>
        Hi Team,<br/>
        I hope you are doing well! <br/>
        Below is the new lead. Details are as given:<br/><br/>
        PersonName: {mainRequest.PersonName}<br/>
        Mobile Number: {mainRequest.MobileNumber}<br/>
        EmailID: {mainRequest.EmailID}<br/>
        UTMSource: {dematSetting.UTMSource}<br/>
        RequestID: {requestID}<br/><br/>
        Please process this request as soon as possible.<br/>
        Thanks,<br/>
        {Company.ClientName} team
    </body>
</html>";

            string failureMessage = "";
            try
            {
                MessageProcessing.SendEmailCoreV2(dematSetting.EmailID, message, Subject, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            return StatusType.Ok;
        }
    }
}