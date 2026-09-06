using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class BeneficiaryOTPBAL
    {
        public string Insert(Beneficiary beneficiary, int UserID)
        {
            try
            {
                BeneficiaryOTP ObjModel = new BeneficiaryOTP
                {
                    SenderMobile = beneficiary.SenderMobile,
                    OTP = new Util().GetRandomOTP(),
                    SenderID = beneficiary.SenderID,
                    BeneficiaryID = beneficiary.BeneficiaryID,
                    UserID = UserID,
                    IsOTPVerified = false,
                };

                int affectedRows = new BeneficiaryOTPDAL().Insert(ObjModel);

                if (affectedRows <= 0)
                {
                    return SenderMessage.InsertFailure;
                }
                Response.MessageSetting.ViewModel setting = new MessageSettingDal().SelectByType(Enum.GetName(typeof(MessageSettingType), MessageSettingType.SenderRegistration));

                if (setting == null) return SenderMessage.InsertFailure;

                string message = setting.SmsMessage;
                message = message.Replace("[OTP]", ObjModel.OTP);
                message = message.Replace("[FirmName]", beneficiary.Name);
                message = message.Replace("[SystemName]", Company.ClientName);

                SentSMS objModel = new SentSMS
                {
                    Mobile = beneficiary.SenderMobile,
                    Message = message,
                    UserID = UserID,
                    Type = Enum.GetName(typeof(SMSType), SMSType.Beneficiary),
                    TemplateID = setting?.TemplateID ?? "",
                };

                if (setting.IsSMS) MessageProcessing.SendSMS(objModel, Enum.GetName(typeof(SMSType), SMSType.Beneficiary));

                if (setting.IsWhatsUp)
                {
                    string WhatsUpmessage = setting.WhatsUpMessage;
                    WhatsUpmessage = WhatsUpmessage.Replace("[OTP]", ObjModel.OTP);
                    WhatsUpmessage = WhatsUpmessage.Replace("[FirmName]", beneficiary.Name);
                    WhatsUpmessage = WhatsUpmessage.Replace("[SystemName]", Company.ClientName);

                    if (WhatsUpmessage.Length <= 5)
                    {
                        WhatsUpmessage = message;
                    }

                    SentSMS objModelwp = new SentSMS
                    {
                        Mobile = beneficiary.SenderMobile,
                        WhatsAppNumber = beneficiary.SenderMobile,
                        Message = WhatsUpmessage,
                        UserID = UserID,
                        Type = Enum.GetName(typeof(SMSType), SMSType.Beneficiary),
                        TemplateID = setting?.TemplateID ?? "",
                    };
                    MessageProcessing.SendWhatsAppSMS(objModelwp, Enum.GetName(typeof(SMSType), SMSType.Beneficiary));
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