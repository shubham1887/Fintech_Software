using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class SenderOTPBAL
    {
        internal Response.MessageSetting.ViewModel GetIsSMSOrMailOrWhatsUp(string type)
        {
            Response.MessageSetting.ViewModel setting = new Response.MessageSetting.ViewModel();
            setting.IsMail = true;
            setting.IsSMS = true;
            setting.IsWhatsUp = true;

            setting.TemplateID = "";
            setting.SmsMessage = "";
            setting.EmailMessage = "";
            setting.WhatsUpMessage = "";

            try
            {
                Response.MessageSetting.ViewModel vi = new MessageSettingDal().SelectByType(type);

                if (vi == null)
                    return setting;

                setting.IsMail = vi.IsMail;
                setting.IsSMS = vi.IsSMS;
                setting.IsWhatsUp = vi.IsWhatsUp;

                setting.TemplateID = vi.TemplateID;
                setting.SmsMessage = vi.SmsMessage;
                setting.EmailMessage = vi.EmailMessage;

                setting.WhatsUpMessage = vi.WhatsUpMessage;
                if (vi.WhatsUpMessage.Length <= 2)
                {
                    setting.WhatsUpMessage = vi.SmsMessage;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return setting;
        }

        public string Insert(int UserID, Sender obj)
        {
            try
            {
                SenderOTP ObjModel = new SenderOTP
                {
                    Mobile = obj.Mobile,
                    OTP = new Util().GetRandomOTP(),
                    SenderID = obj.SenderID,
                    UserID = UserID,
                    IsOTPVerified = false,
                };

                int affectedRows = new SenderOTPDAL().Insert(ObjModel);

                if (affectedRows <= 0)
                {
                    return SenderMessage.InsertFailure;
                }
                Response.MessageSetting.ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.SenderRegistration));

                if (setting == null) return SenderMessage.InsertFailure;

                string message = setting.SmsMessage;
                message = message.Replace("[OTP]", ObjModel.OTP);
                message = message.Replace("[FirmName]", obj.Name);
                message = message.Replace("[SystemName]", Company.ClientName);

                SentSMS objModel = new SentSMS
                {
                    Mobile = obj.Mobile,
                    Message = message,
                    UserID = UserID,
                    Type = Enum.GetName(typeof(SMSType), SMSType.Sender),
                    TemplateID = setting?.TemplateID ?? "",
                };

                if (setting.IsSMS) MessageProcessing.SendSMS(objModel, Enum.GetName(typeof(SMSType), SMSType.Sender));

                if (setting.IsWhatsUp)
                {
                    string WhatsUpmessage = setting.WhatsUpMessage;
                    WhatsUpmessage = WhatsUpmessage.Replace("[OTP]", ObjModel.OTP);
                    WhatsUpmessage = WhatsUpmessage.Replace("[FirmName]", obj.Name);
                    WhatsUpmessage = WhatsUpmessage.Replace("[SystemName]", Company.ClientName);

                    if (WhatsUpmessage.Length <= 5)
                    {
                        WhatsUpmessage = message;
                    }

                    SentSMS objModelwp = new SentSMS
                    {
                        Mobile = obj.Mobile,
                        WhatsAppNumber = obj.Mobile,
                        Message = WhatsUpmessage,
                        UserID = UserID,
                        Type = Enum.GetName(typeof(SMSType), SMSType.Sender),
                        TemplateID = setting?.TemplateID ?? "",
                    };
                    MessageProcessing.SendWhatsAppSMS(objModelwp, Enum.GetName(typeof(SMSType), SMSType.Sender));
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return StatusType.Ok;
        }
    }
}