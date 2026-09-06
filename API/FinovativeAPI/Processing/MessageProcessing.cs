using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Response.SmsMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class MessageProcessing
    {
        private static int DOCInt = Convert.ToInt32(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
        private static readonly string SendEmailKEY = "_SendEmail" + DOCInt.ToString();

        private static void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<string>();
            cache.Remove(SendEmailKEY);
        }

        private static string SendSMSCore(string strUrl)
        {
            string dataString = "";
            try
            {
                dataString = HttpProcessing.CallGetMethod(strUrl, 10);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return dataString;
        }

        private static string SendSMSCorePostMethod(string baseURLWithBody, string header)
        {
            string result = "";
            try
            {
                result = HttpProcessing.CallPostMethod(baseURLWithBody, header, 10);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return result;
        }

        public static void SendSMS(SentSMS sentSMSModel, string Type, string MaskedMessage = "")//Create in Common Function
        {
            if (sentSMSModel == null)
                return;// Messages.DataNotFound;

            sentSMSModel.Message = sentSMSModel.Message.Replace("%20", " ");
            sentSMSModel.Message = sentSMSModel.Message.Replace("[SystemName]", Company.ClientName);
            sentSMSModel.Message = sentSMSModel.Message.Replace("[DateTime]", DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt"));

            MaskedMessage = MaskedMessage.Replace("%20", " ");
            sentSMSModel.Message = sentSMSModel.Message.Replace("[SystemName]", Company.ClientName);
            sentSMSModel.Message = sentSMSModel.Message.Replace("[DateTime]", DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt"));
            SmsURLResponse urLResponse = new MessageProcessing().GenerateSmsURL(sentSMSModel, out string failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                sentSMSModel.Response = failureMessage;
                new SentSmsDAL().SMSInsert(sentSMSModel);
                return;//SMSMasterMessage.Urlnotfound;
            }

            List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

            string MaskedPasswordAndOTP = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.MaskedPasswordAndOTP),
                                                                UserMessage.MaskedPasswordAndOTPDefaultValue,
                                                                whiteList);

            if (MaskedPasswordAndOTP.EqualsIgnoreCase(UserMessage.MaskedPasswordAndOTPDefaultValue))
            {
                sentSMSModel.Message = MaskedMessage;
            }

            Task.Run(() =>
            {
                sendByGetOrPost(sentSMSModel, urLResponse);
            });
            //return sentSMSModel.Response;
        }

        public static void SendSMSWITHOTP(SentSMS sentSMSModel, string Type, string MaskedMessage = "")//Create in Common Function
        {
            if (sentSMSModel == null)
                return;// Messages.DataNotFound;

            sentSMSModel.Message = sentSMSModel.Message.Replace("%20", " ");
            sentSMSModel.Message = sentSMSModel.Message.Replace("[SystemName]", Company.ClientName);
            sentSMSModel.Message = sentSMSModel.Message.Replace("[DateTime]", DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt"));

            MaskedMessage = MaskedMessage.Replace("%20", " ");
            sentSMSModel.Message = sentSMSModel.Message.Replace("[SystemName]", Company.ClientName);
            sentSMSModel.Message = sentSMSModel.Message.Replace("[DateTime]", DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt"));
            SmsURLResponse urLResponse = new MessageProcessing().GenerateOTPSMSURL(sentSMSModel, out string failureMessage);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                sentSMSModel.Response = failureMessage;
                new SentSmsDAL().SMSInsert(sentSMSModel);
                return;//SMSMasterMessage.Urlnotfound;
            }

            List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

            string MaskedPasswordAndOTP = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.MaskedPasswordAndOTP),
                                                                UserMessage.MaskedPasswordAndOTPDefaultValue,
                                                                whiteList);

            if (MaskedPasswordAndOTP.EqualsIgnoreCase(UserMessage.MaskedPasswordAndOTPDefaultValue))
            {
                sentSMSModel.Message = MaskedMessage;
            }

            Task.Run(() =>
            {
                sendByGetOrPost(sentSMSModel, urLResponse);
            });
            //return sentSMSModel.Response;
        }

        private static void sendByGetOrPost(SentSMS sentSMSModel, SmsURLResponse urLResponse)
        {
            SentSMS obj = new SentSmsBAL().SelectDuplicateByUrlAndUserID(sentSMSModel, urLResponse.SmsURL);

            if (obj != null)
            {
                return;
            }

            // check duplcite by url, usedid, mobile
            if (urLResponse.MethodType.EqualsIgnoreCase(MethodType.Get))
            {
                sentSMSModel.Response = SendSMSCore(urLResponse.SmsURL);
            }

            if (urLResponse.MethodType.EqualsIgnoreCase(MethodType.Post))
            {
                sentSMSModel.Response = SendSMSCorePostMethod(urLResponse.SmsURL, urLResponse.HeaderJson);
            }
            sentSMSModel.Request = urLResponse.SmsURL;
            new SentSmsDAL().SMSInsert(sentSMSModel);
        }

        public static string CheckSendSMS(SentSMS sentSMSModel, string Type, string MaskedMessage = "")//Create in Common Function
        {
            if (sentSMSModel == null)
                return Messages.DataNotFound;

            sentSMSModel.Message = sentSMSModel.Message.Replace("%20", " ");
            MaskedMessage = MaskedMessage.Replace("%20", " ");

            SmsURLResponse urLResponse = new MessageProcessing().GenerateSmsURL(sentSMSModel, out string failureMessage);

            List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

            string MaskedPasswordAndOTP = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.MaskedPasswordAndOTP),
                UserMessage.MaskedPasswordAndOTPDefaultValue,
                whiteList);

            if (MaskedPasswordAndOTP.EqualsIgnoreCase(UserMessage.MaskedPasswordAndOTPDefaultValue))
            {
                sentSMSModel.Message = MaskedMessage;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                sentSMSModel.Response = failureMessage;
                new SentSmsDAL().SMSInsert(sentSMSModel);
                return SMSMasterMessage.Urlnotfound;
            }

            if (urLResponse.MethodType.EqualsIgnoreCase(MethodType.Get))
            {
                sentSMSModel.Response = SendSMSCore(urLResponse.SmsURL);
            }

            if (urLResponse.MethodType.EqualsIgnoreCase(MethodType.Post))
            {
                sentSMSModel.Response = SendSMSCorePostMethod(urLResponse.SmsURL, urLResponse.HeaderJson);
            }

            new SentSmsDAL().SMSInsert(sentSMSModel);

            return sentSMSModel.Response;
        }

        private SmsURLResponse GenerateSmsURL(SentSMS sentSMS, out string failureMessage)
        {
            SmsURLResponse urLResponse = new SmsURLResponse();
            failureMessage = StatusType.Failure;
            sentSMS.SMSMasterID = 0;

            ViewModel obj = new SmsMasterBal().SelectActive();
            //SMSMaster obj = new SMSDAL().SelectActive();
            if (obj == null)
            {
                failureMessage = "URL not active";
                return urLResponse;
            }

            string activeSMSURL = obj.SMSString;
            sentSMS.SMSMasterID = obj.SMSMasterID;

            if (string.IsNullOrEmpty(activeSMSURL) || activeSMSURL.Trim().Length <= 5)
            {
                failureMessage = "URL blank";
                return urLResponse;
            }

            if (string.IsNullOrEmpty(sentSMS.Mobile))
            {
                failureMessage = "Number blank";
                return urLResponse;
            }

            if (sentSMS.Mobile == null || sentSMS.Mobile.Length <= 5)
            {
                failureMessage = "Wrong Number";
                return urLResponse;
            }

            string smsurl = activeSMSURL.ToString()
                   .Replace("[mobile]", sentSMS.Mobile.UrlEncode())
                   .Replace("[message]", sentSMS.Message.UrlEncode())
                  .Replace("[TemplateID]", sentSMS.TemplateID.UrlEncode());

            urLResponse.SmsURL = smsurl;
            urLResponse.HeaderJson = obj.HeaderJson;
            urLResponse.MethodType = obj.MethodType;

            if (urLResponse.MethodType.Length <= 0)
                urLResponse.MethodType = MethodType.Get;

            failureMessage = StatusType.Ok;
            return urLResponse;
        }

        private SmsURLResponse GenerateOTPSMSURL(SentSMS sentSMS, out string failureMessage)
        {
            SmsURLResponse urLResponse = new SmsURLResponse();
            failureMessage = StatusType.Failure;
            sentSMS.SMSMasterID = 0;

            ViewModel obj = new SmsMasterBal().SelectActive();
            if (obj == null)
            {
                failureMessage = "URL not active";
                return urLResponse;
            }

            string activeSMSURL = obj.SMSOTPURL;
            sentSMS.SMSMasterID = obj.SMSMasterID;

            if (string.IsNullOrEmpty(activeSMSURL) || activeSMSURL.Trim().Length <= 5)
            {
                failureMessage = "URL blank";
                return urLResponse;
            }

            if (string.IsNullOrEmpty(sentSMS.Mobile))
            {
                failureMessage = "Number blank";
                return urLResponse;
            }

            if (sentSMS.Mobile == null || sentSMS.Mobile.Length <= 5)
            {
                failureMessage = "Wrong Number";
                return urLResponse;
            }

            string smsurl = activeSMSURL.ToString()
                   .Replace("[mobile]", sentSMS.Mobile.UrlEncode())
                   .Replace("[message]", sentSMS.Message.UrlEncode())
                  .Replace("[TemplateID]", sentSMS.TemplateID.UrlEncode());

            urLResponse.SmsURL = smsurl;
            urLResponse.HeaderJson = obj.HeaderJson;
            urLResponse.MethodType = obj.MethodType;

            if (urLResponse.MethodType.Length <= 0)
                urLResponse.MethodType = MethodType.Get;

            failureMessage = StatusType.Ok;
            return urLResponse;
        }

        private static bool IsValidEmailCore(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidEmail(string To, string Message, string Subject)
        {
            //EmailID, Message, Subject
            Validation valid = new Validation();
            if (!valid.IsValidString(Subject))
                return false;
            if (!valid.IsValidString(Message))
                return false;
            if (!valid.IsValidString(To))
                return false;

            if (!new Validation().IsValidEmail(To))
            {
                return false;
            }

            if (!IsValidEmailCore(To))
                return false;

            return true;
        }

        private static void SendEmailCore(string EmailID, string Message, string Subject, string Attachment = "")
        {
            var sendEmailError = new MemoryCacheProcessing<string>();
            string errorMessage = sendEmailError.OnlyGet(SendEmailKEY);

            if (errorMessage != null && errorMessage.Length > 5)
            {
                return;
            }

            string fromAddress = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.EmailIDForSend));
            string fromPassword = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.EmailPassword));
            string SMTPPort = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SMTPPort));
            string SMTPHost = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SMTPHost));

            if (!new Validation().IsValidEmail(fromAddress))
            {
                return;
            }

            using (System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage())
            {
                try
                {
                    if (!IsValidEmail(EmailID, Message, Subject)) return;

                    mailMessage.From = new MailAddress(fromAddress, Company.ClientName);
                    mailMessage.Subject = Subject;
                    mailMessage.Body = Message;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.To.Add(EmailID);

                    if (!string.IsNullOrEmpty(Attachment))
                    {
                        try
                        {
                            mailMessage.Attachments.Add(GetAttachmentFilePath(Attachment));
                        }
                        catch (Exception)
                        {
                            Attachment data = new Attachment(Attachment, MediaTypeNames.Application.Octet);
                            mailMessage.Attachments.Add(data);
                        }
                    }

                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.EnableSsl = true;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);
                        smtp.Host = SMTPHost;
                        smtp.Port = Convert.ToInt32(SMTPPort);
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                        smtp.Send(mailMessage);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("Mailbox unavailable. The server response was: 5.4.5 Daily user sending quota exceeded".ToLower()))
                    {
                        RemoveCache();
                        var emailError = new MemoryCacheProcessing<string>();
                        emailError.GetOrCreate(SendEmailKEY, () => ex.Message);
                        return;
                    }
                    new Logger().Write(ex);
                }
            }

        }

        public static int SendEmailCoreV2(string EmailID, string Message, string Subject, out string failureMessage, string Attachment = "")
        {
            int Result = 0;

            var sendEmailError = new MemoryCacheProcessing<string>();
            string errorMessage = sendEmailError.OnlyGet(SendEmailKEY);

            if (errorMessage != null && errorMessage.Length > 5)
            {
                failureMessage = errorMessage;
                return 0;
            }

            string fromAddress = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.EmailIDForSend));
            string fromPassword = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.EmailPassword));
            string SMTPPort = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SMTPPort));
            string SMTPHost = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SMTPHost));

            if (!new Validation().IsValidEmail(fromAddress))
            {
                failureMessage = "Invalid email setting";
                return 0;
            }

            using (System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage())
            {
                try
                {
                    if (!IsValidEmail(EmailID, Message, Subject))
                    {
                        failureMessage = "Invalid email";
                        return Result;
                    }
                    mailMessage.From = new MailAddress(fromAddress, Company.ClientName);
                    mailMessage.Subject = Subject;
                    mailMessage.Body = Message;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.To.Add(EmailID);

                    if (!string.IsNullOrEmpty(Attachment))
                    {
                        try
                        {
                            mailMessage.Attachments.Add(GetAttachmentFilePath(Attachment));
                        }
                        catch (Exception)
                        {
                            Attachment data = new Attachment(Attachment, MediaTypeNames.Application.Octet);
                            mailMessage.Attachments.Add(data);
                        }
                    }

                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.EnableSsl = true;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);
                        smtp.Host = SMTPHost;
                        smtp.Port = Convert.ToInt32(SMTPPort);
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                        smtp.Send(mailMessage);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("Mailbox unavailable. The server response was: 5.4.5 Daily user sending quota exceeded".ToLower()))
                    {
                        RemoveCache();
                        var emailError = new MemoryCacheProcessing<string>();
                        emailError.GetOrCreate(SendEmailKEY, () => ex.Message);
                        failureMessage = ex.Message;
                        return Result;
                    }
                    new Logger().Write(ex);
                }
            }
            failureMessage = StatusType.Ok;
            return Result;
        }

        private static Attachment GetAttachmentFilePath(string attachment)
        {
            if (attachment.Contains(Company.WebAPIURL))
            {
                attachment = attachment.Replace(Company.WebAPIURL, "");
            }
            attachment = attachment.Replace("app-docs", "");
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"Media/");
            path += attachment;
            path = path.Replace("/", "\\");
            Attachment data = new Attachment(path, MediaTypeNames.Application.Octet);
            return data;
        }

        public static void SendEmail(string EmailID, string mail, string Subject, string Attachment = "")
        {
            try
            {
                mail = mail.Replace("[SystemName]", Company.ClientName);
                mail = mail.Replace("[DateTime]", DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt"));

                SendEmailCore(EmailID, mail, Subject, Attachment);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        public string CheckEmail(string EmailID, string Message, string Subject)
        {
            string Response = "";
            try
            {
                Response = CheckEmailCore(EmailID, Message, Subject);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return Response;
        }

        private static string CheckEmailCore(string EmailID, string Message, string Subject, string Attachment = "")
        {
            string Response = "";

            string fromAddress = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.EmailIDForSend));
            string fromPassword = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.EmailPassword));
            string SMTPPort = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SMTPPort));
            string SMTPHost = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.SMTPHost));

            if (!new Validation().IsValidEmail(fromAddress))
            {
                return "Invalid emailID:" + fromAddress;
            }

            using (System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage())
            {
                try
                {
                    if (!IsValidEmail(EmailID, Message, Subject)) return Response;

                    mailMessage.From = new MailAddress(fromAddress, Company.ClientName);
                    mailMessage.Subject = Subject;
                    mailMessage.Body = Message;
                    // mailMessage.IsBodyHtml = true;
                    mailMessage.To.Add(EmailID);

                    if (!string.IsNullOrEmpty(Attachment))
                    {
                        mailMessage.Attachments.Add(GetAttachmentFilePath(Attachment));
                    }

                    SmtpClient smtp = new SmtpClient
                    {
                        EnableSsl = true,
                        Host = SMTPHost
                    };
                    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential
                    {
                        UserName = fromAddress,
                        Password = fromPassword
                    };

                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = Convert.ToInt32(SMTPPort);
                    smtp.Send(mailMessage);
                    Response = "Mail sent successfully";
                }
                catch (Exception ex)
                {
                    Response = ex.Message.ToString();
                    new Logger().Write(ex);
                }
            }
            return Response;
        }

        public static void SendWhatsAppSMS(SentSMS sentSMSModel, string Type, string MaskedMessage = "")//Create in Common Function
        {
            if (sentSMSModel == null)
                return;// Messages.DataNotFound;

            sentSMSModel.Message = sentSMSModel.Message.Replace("%20", " ");
            sentSMSModel.Message = sentSMSModel.Message.Replace("[SystemName]", Company.ClientName);
            sentSMSModel.Message = sentSMSModel.Message.Replace("[DateTime]", DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt"));

            MaskedMessage = MaskedMessage.Replace("%20", " ").Replace("[SystemName]", Company.ClientName);
            sentSMSModel.Message = sentSMSModel.Message.Replace("[SystemName]", Company.ClientName);
            sentSMSModel.Message = sentSMSModel.Message.Replace("[DateTime]", DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt"));

            SmsURLResponse urLResponse = GenerateWhatsAppURL(sentSMSModel, out string failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                sentSMSModel.Response = failureMessage;
                new SentSmsDAL().SMSInsert(sentSMSModel);
                return;// SMSMasterMessage.Urlnotfound;
            }

            List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

            string MaskedPasswordAndOTP = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.MaskedPasswordAndOTP),
                                                                UserMessage.MaskedPasswordAndOTPDefaultValue,
                                                                whiteList);

            if (MaskedPasswordAndOTP.EqualsIgnoreCase(UserMessage.MaskedPasswordAndOTPDefaultValue))
            {
                sentSMSModel.Message = MaskedMessage;
            }

            Task.Run(() =>
            {
                if (urLResponse.MethodType.EqualsIgnoreCase(MethodType.Get))
                {
                    sentSMSModel.Response = SendSMSCore(urLResponse.SmsURL);
                }

                if (urLResponse.MethodType.EqualsIgnoreCase(MethodType.Post))
                {
                    sentSMSModel.Response = SendSMSCorePostMethod(urLResponse.SmsURL, urLResponse.HeaderJson);
                }
                sentSMSModel.Request = urLResponse.SmsURL;
                new SentSmsDAL().SMSInsert(sentSMSModel);
            });

        }

        private static SmsURLResponse GenerateWhatsAppURL(SentSMS sentSMS, out string failureMessage)
        {
            SmsURLResponse urLResponse = new SmsURLResponse();
            failureMessage = StatusType.Failure;
            sentSMS.SMSMasterID = 0;

            ViewModel obj = new SmsMasterBal().SelectActive();
            //SMSMaster obj = new SMSDAL().SelectActive();

            if (obj == null)
            {
                failureMessage = "URL not active";
                return urLResponse;
            }

            string activeWhatsAppURL = obj.WhatsAppURL;
            string MobileWithOrWithout91 = obj.MobileWithOrWithout91;
            sentSMS.SMSMasterID = obj.SMSMasterID;

            if (string.IsNullOrEmpty(activeWhatsAppURL) || activeWhatsAppURL.Length <= 5)
            {
                failureMessage = "WhatsAppURL blank";
                return urLResponse;
            }

            string number = sentSMS.WhatsAppNumber;
            if (sentSMS.WhatsAppNumber == null || number.Length <= 2)
            {
                number = sentSMS.Mobile;
            }

            if (string.IsNullOrEmpty(number))
            {
                failureMessage = "Number blank";
                return urLResponse;
            }

            if (number.Length <= 5)
            {
                failureMessage = "Wrong Number";
                return urLResponse;
            }

            if (MobileWithOrWithout91 == "Yes")
            {
                number = "91" + number;
            }

            string whatsAppURL = activeWhatsAppURL.ToString()
                   .Replace("[mobile]", number.UrlEncode())
                   .Replace("[message]", sentSMS.Message.UrlEncode())
                   .Replace("[TemplateID]", sentSMS.TemplateID.UrlEncode());

            urLResponse.SmsURL = whatsAppURL;
            urLResponse.HeaderJson = obj.HeaderJsonWhatsApp;
            urLResponse.MethodType = obj.MethodTypeWhatsApp;

            if (urLResponse.MethodType.Length <= 0)
                urLResponse.MethodType = MethodType.Get;

            failureMessage = StatusType.Ok;
            return urLResponse;
        }

        public string CheckWhatsAppSms(string WhatsAppNumber, string Message)
        {
            string type = Enum.GetName(typeof(SMSType), SMSType.Testing);
            string MaskedMessage = Message;
            SentSMS objModel = new SentSMS
            {
                WhatsAppNumber = WhatsAppNumber,
                Mobile = WhatsAppNumber,
                Message = Message,
                UserID = Company.AdminUserID,
                Type = type
            };

            string Response = CheckWhatsAppSendSMS(objModel, objModel.Type, MaskedMessage);
            return Response;
        }

        public static string CheckWhatsAppSendSMS(SentSMS sentSMSModel, string Type, string MaskedMessage = "")//Create in Common Function
        {
            if (sentSMSModel == null)
                return Messages.DataNotFound;

            sentSMSModel.Message = sentSMSModel.Message.Replace("%20", " ").Replace("[SystemName]", Company.ClientName);
            sentSMSModel.Message = sentSMSModel.Message.Replace("[DateTime]", DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt"));

            MaskedMessage = MaskedMessage.Replace("%20", " ").Replace("[SystemName]", Company.ClientName);
            MaskedMessage = MaskedMessage.Replace("[DateTime]", DateTime.Now.ToString("dd/MMM/yyyy hh:mm tt"));

            SmsURLResponse urLResponse = GenerateWhatsAppURL(sentSMSModel, out string failureMessage);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                sentSMSModel.Response = failureMessage;
                new SentSmsDAL().SMSInsert(sentSMSModel);
                return SMSMasterMessage.Urlnotfound;
            }

            List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

            string MaskedPasswordAndOTP = new SettingMasterBal().SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.MaskedPasswordAndOTP),
                UserMessage.MaskedPasswordAndOTPDefaultValue,
                whiteList);

            if (MaskedPasswordAndOTP.EqualsIgnoreCase(UserMessage.MaskedPasswordAndOTPDefaultValue))
            {
                sentSMSModel.Message = MaskedMessage;
            }

            if (urLResponse.MethodType.EqualsIgnoreCase(MethodType.Get))
            {
                sentSMSModel.Response = SendSMSCore(urLResponse.SmsURL);
            }

            if (urLResponse.MethodType.EqualsIgnoreCase(MethodType.Post))
            {
                sentSMSModel.Response = SendSMSCorePostMethod(urLResponse.SmsURL, urLResponse.HeaderJson);
            }

            sentSMSModel.Request = urLResponse.SmsURL;
            new SentSmsDAL().SMSInsert(sentSMSModel);

            return sentSMSModel.Response;
        }
    }
}