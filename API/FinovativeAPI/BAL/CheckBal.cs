using Project.Message;
using Project.Processing;
using Project.Requests.Check;
using Project.StaticModel;

namespace Project.BAL
{
    public class CheckBal
    {
        internal string CheckSms(SmsRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            string Response = new MessageSelection().CheckSMS(mainRequest.MobileNumber, mainRequest.Message);
            return Response;
        }

        internal string CheckEmail(EmailRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            string Response = new MessageProcessing().CheckEmail(mainRequest.EmailID, mainRequest.Message, CheckMessage.Subject);
            return Response;
        }

        internal string CheckWhatsAppSms(WhatsAppSmsRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            string Response = new MessageProcessing().CheckWhatsAppSms(mainRequest.WhatsAppNumber, mainRequest.Message);
            return Response;
        }
    }
}