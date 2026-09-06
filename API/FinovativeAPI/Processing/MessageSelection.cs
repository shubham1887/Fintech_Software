using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Response.MessageSetting;
using Project.Response.User;
using Project.StaticModel;
using System;

namespace Project.Processing
{
    public class MessageSelection
    {
        internal ViewModel GetIsSMSOrMailOrWhatsUp(string type)
        {
            ViewModel setting = new ViewModel();
            setting.IsMail = true;
            setting.IsSMS = true;
            setting.IsWhatsUp = true;
            setting.ISOTP = true;

            setting.TemplateID = "";
            setting.SmsMessage = "";
            setting.EmailMessage = "";
            setting.WhatsUpMessage = "";

            try
            {
                ViewModel vi = new MessageSettingDal().SelectByType(type);

                if (vi == null)
                    return setting;

                setting.IsMail = vi.IsMail;
                setting.IsSMS = vi.IsSMS;
                setting.IsWhatsUp = vi.IsWhatsUp;
                setting.ISOTP = vi.ISOTP;


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

        public void SendSignupMessage(int userID, string Password, string MobileNumber, string WhatsAppNumber, string FirmName, string EmailID, string Mpin)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.Signup));

            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", FirmName);
            message = message.Replace("[MobileNumber]", MobileNumber);
            message = message.Replace("[Pin]", Mpin);
            string MaskedMessage = message;
            message = message.Replace("[Password]", Password);

            string type = Enum.GetName(typeof(SMSType), SMSType.SignUP);
            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                WhatsAppNumber = WhatsAppNumber,
                Message = message,
                UserID = userID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, type, MaskedMessage);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);
            mail = mail.Replace("[MobileNumber]", MobileNumber);
            mail = mail.Replace("[Password]", Password);
            mail = mail.Replace("[Pin]", Mpin);

            if (setting.IsMail) MessageProcessing.SendEmail(EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string WhatsUpmessage = setting.WhatsUpMessage;
                WhatsUpmessage = WhatsUpmessage.Replace("[FirmName]", FirmName);
                WhatsUpmessage = WhatsUpmessage.Replace("[MobileNumber]", MobileNumber);
                WhatsUpmessage = WhatsUpmessage.Replace("[Pin]", Mpin);
                string MaskedWhatsUpMessage = WhatsUpmessage;
                WhatsUpmessage = WhatsUpmessage.Replace("[Password]", Password);

                if (WhatsUpmessage.Length <= 5)
                {
                    WhatsUpmessage = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    Mobile = MobileNumber,
                    WhatsAppNumber = WhatsAppNumber,
                    Message = WhatsUpmessage,
                    UserID = userID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, type, MaskedWhatsUpMessage);
            }
        }

        public void SendLeadGenerationMessage(string MobileNumber, string EmailID)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.LeadGenerationMessage));

            string message = setting.SmsMessage;
            string MaskedMessage = message;

            string type = Enum.GetName(typeof(SMSType), SMSType.LeadGenerationMessage);
            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                Message = message,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, type, MaskedMessage);

            string mail = setting.EmailMessage;

            if (setting.IsMail) MessageProcessing.SendEmail(EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string WhatsUpmessage = setting.WhatsUpMessage;
                string MaskedWhatsUpMessage = WhatsUpmessage;

                if (WhatsUpmessage.Length <= 5)
                {
                    WhatsUpmessage = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    Mobile = MobileNumber,
                    Message = WhatsUpmessage,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, type, MaskedWhatsUpMessage);
            }
        }


        public void SendSignupMailToAdmin(string MobileNumber, string FirmName)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.SignUpMailForAdmin));
            string type = Enum.GetName(typeof(SMSType), SMSType.SignUP);
            string AdminEmailID = new UserDAL().SelectAdminEmailID();

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);
            mail = mail.Replace("[MobileNumber]", MobileNumber);

            MessageProcessing.SendEmail(AdminEmailID, mail, type);
        }

        public void SendPasswordMessage(string Password, int UserID, string MobileNumber, string WhatsAppNumber, string EmailID, string FirmName)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.SendPassword));

            string type = Enum.GetName(typeof(SMSType), SMSType.SendPassword);
            string message = setting.SmsMessage;

            message = message.Replace("[MobileNumber]", MobileNumber);
            message = message.Replace("[FirmName]", FirmName);

            string MaskedMessage = message;
            message = message.Replace("[Password]", Password);

            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                WhatsAppNumber = WhatsAppNumber,
                Message = message,
                UserID = UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
            {
                MessageProcessing.SendSMS(objModel, objModel.Type, MaskedMessage);
            }

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);
            mail = mail.Replace("[MobileNumber]", MobileNumber);
            mail = mail.Replace("[Password]", Password);

            if (setting.IsMail)
            {
                MessageProcessing.SendEmail(EmailID, mail, type);
            }

            if (setting.IsWhatsUp)
            {
                string Wpmessage = setting.WhatsUpMessage;

                Wpmessage = Wpmessage.Replace("[FirmName]", FirmName);
                Wpmessage = Wpmessage.Replace("[MobileNumber]", MobileNumber);

                string WpMaskedMessage = Wpmessage;
                Wpmessage = Wpmessage.Replace("[Password]", Password);

                if (Wpmessage.Length <= 5)
                {
                    Wpmessage = message;
                }
                SentSMS objModelwp = new SentSMS
                {
                    Mobile = MobileNumber,
                    WhatsAppNumber = WhatsAppNumber,
                    Message = Wpmessage,
                    UserID = UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModel.Type, WpMaskedMessage);
            }
        }

        public void ChangePasswordMessage(int UserID, string MobileNumber, string WhatsAppNumber, string EmailID, string FirmName)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.ChangePassword));

            string type = Enum.GetName(typeof(SMSType), SMSType.ChangePassword);
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", FirmName);
            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                WhatsAppNumber = WhatsAppNumber,
                Message = message,
                UserID = UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
            {
                MessageProcessing.SendSMS(objModel, objModel.Type, message);
            }

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);

            if (setting.IsMail)
            {
                MessageProcessing.SendEmail(EmailID, mail, type);
            }

            if (setting.IsWhatsUp)
            {
                string wpmessage = setting.WhatsUpMessage;
                wpmessage = wpmessage.Replace("[FirmName]", FirmName);

                if (wpmessage.Length <= 5)
                {
                    wpmessage = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    Mobile = MobileNumber,
                    WhatsAppNumber = WhatsAppNumber,
                    Message = wpmessage,
                    UserID = UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModel.Type, wpmessage);
            }
        }

        public void SendLoginOTP(LoginResponse response)
        {
            UserMaster user = new UserDAL().SelectByIDFromCache(response.UserID);
            if (user == null)
                return;
            string type = Enum.GetName(typeof(SMSType), SMSType.Login);
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.LoginOTP));

            OTPCore(user, setting, type);
        }


        public void SendForgotPasswordOTP(UserMaster user)
        {
            string type = Enum.GetName(typeof(SMSType), SMSType.ForgotPasswordOTP);
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.ForgotPasswordOTP));
            OTPCore(user, setting, type);
        }

        public void SendPinOTP(UserMaster user)
        {
            string type = Enum.GetName(typeof(SMSType), SMSType.PinOTP);
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.PinOTP));
            OTPCore(user, setting, type);
        }

        public int SendSignUPPasswordOTP(string MobileNumber)
        {
            string type = Enum.GetName(typeof(SMSType), SMSType.SignUPOtp);
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.SignUPOtp));
            return SignUpOTPCore(MobileNumber, setting, type);
        }

        public void SendOTPForIPMaster(UserMaster user)
        {
            string type = Enum.GetName(typeof(SMSType), SMSType.IPMaster);
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.IPMaster));
            OTPCore(user, setting, type);
        }
        public void SendOTPForSerivesActiveDeActive(UserMaster user)
        {
            string type = Enum.GetName(typeof(SMSType), SMSType.ServiceActivateDeactivate);
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.ServiceActivateDeactivate));
            OTPCore(user, setting, type);
        }
        public void ForgetPasswordMessage(string Password, int UserID, string MobileNumber, string WhatsAppNumber, string EmailID, string FirmName)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.ForgetPassword));

            string type = Enum.GetName(typeof(SMSType), SMSType.ForgetPassword);
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", FirmName);
            string MaskedMessage = message;
            message = message.Replace("[Password]", Password);

            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                Message = message,
                UserID = UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS) MessageProcessing.SendSMS(objModel, objModel.Type, MaskedMessage);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);
            mail = mail.Replace("[Password]", Password);

            if (setting.IsMail)
                MessageProcessing.SendEmail(EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", FirmName);
                string MaskedMessagewp = messagewp;
                messagewp = messagewp.Replace("[Password]", Password);

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    Mobile = MobileNumber,
                    WhatsAppNumber = WhatsAppNumber,
                    Message = messagewp,
                    UserID = UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };

                MessageProcessing.SendWhatsAppSMS(objModelwp, objModel.Type, MaskedMessagewp);
            }
        }

        public void SendAutoBalanceTransferOTP(UserMaster user)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.AutoBalanceTransfer));
            string type = Enum.GetName(typeof(SMSType), SMSType.AutoBalanceTransferOTP);
            OTPCore(user, setting, type);
        }

        public void SendVirtulBalanceOTP(UserMaster user)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.VirtualBalanceOTPMessage));
            string type = Enum.GetName(typeof(SMSType), SMSType.VirtualBalance);
            OTPCore(user, setting, type);
        }

        public void SendFundReverseOTP(UserMaster user)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.FundReverseOTP));
            string type = Enum.GetName(typeof(SMSType), SMSType.FundReverseOTP);
            OTPCore(user, setting, type);
        }

        private bool isDuplicateDMTSMS(SentSMS objModel)
        {
            SentSMS obj = new SentSmsBAL().SelectForDuplicate(objModel.UserID, objModel.Mobile, objModel.Type, objModel.TemplateID);

            if (obj != null)
            {
                return true;
            }

            return false;
        }

        internal void SendDMTTransactionSucessAllOneMessage(TransactionMaster transactionMaster)
        {
            if (!transactionMaster.Status.EqualsIgnoreCase(StatusType.Success))
                return;

            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(transactionMaster.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (user.UserTypeID == (int)UserType.APIUser)
                return;

            ViewModel setting = new ViewModel();

            if (transactionMaster.Status.EqualsIgnoreCase(StatusType.Success))
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.DMTTransactionSuccess));

            if (setting == null)
                return;

            string name = transactionMaster.DMT_BeneficaryNameByBank.TrimIfNotNull();
            if (name.Length <= 0)
            {
                name = transactionMaster.DMT_BeneficiaryName;
            }

            string type = "DMT Message";
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", transactionMaster.DMT_SenderName);
            message = message.Replace("[Status]", transactionMaster.Status);
            message = message.Replace("[AccountNo]", transactionMaster.ConsumerNumber);
            message = message.Replace("[Amount]", transactionMaster.DMT_OriginalAmount.TwoDigitDecimal());
            message = message.Replace("[TxnID]", transactionMaster.ReferenceNo);
            message = message.Replace("[Name]", name);
            message = message.Replace("[BeneficiaryName]", name);
            message = message.Replace("[PaymentType]", transactionMaster.DMT_PaymentType);
            message = message.Replace("[BankName]", transactionMaster.DMT_Bank);
            message = message.Replace("[DateTime]", DateTime.Now.ToString("dd/MMM/yyyy"));

            SentSMS objModel = new SentSMS
            {
                Mobile = transactionMaster.DMT_SenderNumber,
                Message = message,
                UserID = transactionMaster.UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };
            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, type);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", transactionMaster.DMT_SenderName);
            mail = mail.Replace("[Status]", transactionMaster.Status);
            mail = mail.Replace("[AccountNo]", transactionMaster.ConsumerNumber);
            mail = mail.Replace("[TxnID]", transactionMaster.ReferenceNo);
            mail = mail.Replace("[Amount]", transactionMaster.DMT_OriginalAmount.TwoDigitDecimal());
            mail = mail.Replace("[Name]", name);
            mail = mail.Replace("[BeneficiaryName]", name);
            mail = mail.Replace("[PaymentType]", transactionMaster.DMT_PaymentType);
            mail = mail.Replace("[BankName]", transactionMaster.DMT_Bank);

            if (setting.IsMail)
                MessageProcessing.SendEmail(user.EmailID, mail, type);

            string messagewp = setting.WhatsUpMessage;
            messagewp = messagewp.Replace("[FirmName]", transactionMaster.DMT_SenderName);
            messagewp = messagewp.Replace("[Status]", transactionMaster.Status);
            messagewp = messagewp.Replace("[AccountNo]", transactionMaster.ConsumerNumber);
            messagewp = messagewp.Replace("[TxnID]", transactionMaster.ReferenceNo);
            messagewp = messagewp.Replace("[Amount]", transactionMaster.DMT_OriginalAmount.TwoDigitDecimal());
            messagewp = messagewp.Replace("[Name]", name);
            messagewp = messagewp.Replace("[BeneficiaryName]", name);
            messagewp = messagewp.Replace("[PaymentType]", transactionMaster.DMT_PaymentType);
            messagewp = messagewp.Replace("[BankName]", transactionMaster.DMT_Bank);

            if (messagewp.Length <= 5)
            {
                messagewp = message;
            }

            SentSMS objModelwp = new SentSMS
            {
                WhatsAppNumber = user.WhatsAppNumber,
                Mobile = transactionMaster.DMT_SenderNumber,
                Message = messagewp,
                UserID = transactionMaster.UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsWhatsUp)
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModelwp.Type);
        }

        internal void SendDMTTransactionMessage(TransactionMaster transactionMaster)
        {
            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(transactionMaster.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (user.UserTypeID == (int)UserType.APIUser)
                return;

            ViewModel setting = new ViewModel();

            if (transactionMaster.Status.EqualsIgnoreCase(StatusType.Success))
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.DMTTransactionSuccess));

            if (transactionMaster.Status.EqualsIgnoreCase(StatusType.Process))
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.DMTTransactionProcess));

            if (setting == null)
                return;

            string name = transactionMaster.DMT_BeneficaryNameByBank.TrimIfNotNull();
            if (name.Length <= 0)
            {
                name = transactionMaster.DMT_BeneficiaryName;
            }

            string type = "DMT Message";
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", transactionMaster.DMT_SenderName);
            message = message.Replace("[AccountNo]", transactionMaster.ConsumerNumber);
            message = message.Replace("[Amount]", transactionMaster.Amount.TwoDigitDecimal());
            message = message.Replace("[TxnID]", transactionMaster.ReferenceNo);
            message = message.Replace("[Name]", name);
            message = message.Replace("[BeneficiaryName]", name);
            message = message.Replace("[PaymentType]", transactionMaster.DMT_PaymentType);
            message = message.Replace("[BankName]", transactionMaster.DMT_Bank);
            message = message.Replace("[DateTime]", DateTime.Now.ToString("dd/MMM/yyyy"));

            SentSMS objModel = new SentSMS
            {
                Mobile = transactionMaster.DMT_SenderNumber,
                Message = message,
                UserID = transactionMaster.UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };
            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, type);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", transactionMaster.DMT_SenderName);
            mail = mail.Replace("[Status]", transactionMaster.Status);
            mail = mail.Replace("[AccountNo]", transactionMaster.ConsumerNumber);
            mail = mail.Replace("[TxnID]", transactionMaster.ReferenceNo);
            mail = mail.Replace("[Amount]", transactionMaster.Amount.TwoDigitDecimal());
            mail = mail.Replace("[Name]", name);
            mail = mail.Replace("[BeneficiaryName]", name);
            mail = mail.Replace("[PaymentType]", transactionMaster.DMT_PaymentType);
            mail = mail.Replace("[BankName]", transactionMaster.DMT_Bank);

            if (setting.IsMail)
                MessageProcessing.SendEmail(user.EmailID, mail, type);

            string messagewp = setting.WhatsUpMessage;
            messagewp = messagewp.Replace("[FirmName]", transactionMaster.DMT_SenderName);
            messagewp = messagewp.Replace("[Status]", transactionMaster.Status);
            messagewp = messagewp.Replace("[AccountNo]", transactionMaster.ConsumerNumber);
            messagewp = messagewp.Replace("[TxnID]", transactionMaster.ReferenceNo);
            messagewp = messagewp.Replace("[Amount]", transactionMaster.Amount.TwoDigitDecimal());
            messagewp = messagewp.Replace("[Name]", name);
            messagewp = messagewp.Replace("[BeneficiaryName]", name);
            messagewp = messagewp.Replace("[PaymentType]", transactionMaster.DMT_PaymentType);
            messagewp = messagewp.Replace("[BankName]", transactionMaster.DMT_Bank);

            if (messagewp.Length <= 5)
            {
                messagewp = message;
            }

            SentSMS objModelwp = new SentSMS
            {
                WhatsAppNumber = user.WhatsAppNumber,
                Mobile = transactionMaster.DMT_SenderNumber,
                Message = messagewp,
                UserID = transactionMaster.UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsWhatsUp)
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModelwp.Type);
        }

        public void SendKYCApproveRejectMessage(bool IsApproved, UserMaster user, string Remark)
        {
            ViewModel setting = null;

            if (IsApproved)
            {
                setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.KYCAccept));
            }
            else
            {
                setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.KYCReject));
            }

            string subject = (IsApproved) ? KYCMessage.KYC_Accept : KYCMessage.KYC_Reject;

            string sms = setting.SmsMessage;
            sms = sms.Replace("[FirmName]", user.FirmName);
            sms = sms.Replace("[Remark]", Remark);

            SentSMS objModel = new SentSMS
            {
                WhatsAppNumber = user.WhatsAppNumber,
                Mobile = user.MobileNumber,
                Message = sms,
                UserID = user.UserID,
                Type = subject,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, subject, sms);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", user.FirmName);
            mail = mail.Replace("[Remark]", Remark);
            MessageProcessing.SendEmail(user.EmailID, mail, subject);

            if (setting.IsWhatsUp)
            {
                string smswp = setting.WhatsUpMessage;
                smswp = smswp.Replace("[FirmName]", user.FirmName);
                smswp = smswp.Replace("[Remark]", Remark);

                if (smswp.Length <= 5)
                {
                    smswp = sms;
                }

                SentSMS objModelwp = new SentSMS
                {
                    WhatsAppNumber = user.WhatsAppNumber,
                    Mobile = user.MobileNumber,
                    Message = smswp,
                    UserID = user.UserID,
                    Type = subject,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, subject, smswp);
            }
        }

        public int SendKYCPendingMail(UserMaster user)
        {
            string EmailID = new UserDAL().SelectAdminEmailID();

            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.KYCUpload));

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", user.FirmName);

            MessageProcessing.SendEmail(EmailID, mail, "KYC Application");

            UserMaster admin = new UserDAL().SelectByIDFromCache(Company.AdminUserID);
            try
            {
                new FCMProcessing().SendNoticationMessage(admin.AppFCMToken, Message.FundMessage.RefreshBalanceFlag);
                new FCMProcessing().SendNoticationMessage(admin.WebFCMToken, Message.FundMessage.RefreshBalanceFlag);
            }
            catch (Exception)
            {
            }
            return 1;
        }

        internal void SendOTPForAPIToken(UserMaster user)
        {
            string type = Enum.GetName(typeof(SMSType), SMSType.APIToken);
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.APIToken));
            OTPCore(user, setting, type);
        }

        internal void SendOTPForSecureIPOTP(UserMaster user)
        {
            string type = Enum.GetName(typeof(SMSType), SMSType.SecureIPOTP);
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.SecureIPOTP));
            OTPCore(user, setting, type);
        }

        internal void SendBulkMessage(UserViewModel user, string message, bool isSMS, bool isEmail, bool IsWhatsUp)
        {

            string type = Enum.GetName(typeof(SMSType), SMSType.BulkMessage);

            SentSMS objModel = new SentSMS
            {
                Mobile = user.MobileNumber,
                WhatsAppNumber = user.WhatsAppNumber,
                Message = message,
                UserID = user.UserID,
                Type = type
            };

            if (isSMS)
            {
                //   MessageProcessing.SendSMS(objModel, objModel.Type, MaskedMessage);
            }

            if (isEmail)
            {
                MessageProcessing.SendEmail(user.EmailID, message, type);
            }

            if (IsWhatsUp)
            {
                MessageProcessing.SendWhatsAppSMS(objModel, objModel.Type, message);
            }
        }

        internal void SendCollectionMessage(UserMaster user, decimal Outstanding)
        {
            if (user == null) return;

            string type = Enum.GetName(typeof(SMSType), SMSType.CollectionMessage);
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.Outstanding));

            string MobileNumber = user.MobileNumber;
            string WhatsAppNumber = user.WhatsAppNumber;
            string FirmName = user.FirmName;
            string EmailID = user.EmailID;

            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", FirmName);
            message = message.Replace("[Outstanding]", Outstanding.TwoDigitDecimal());
            string MaskedMessage = message;
            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                WhatsAppNumber = WhatsAppNumber,
                Message = message,
                UserID = user.UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, type, MaskedMessage);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);
            mail = mail.Replace("[Outstanding]", Outstanding.TwoDigitDecimal());
            if (setting.IsMail)
                MessageProcessing.SendEmail(EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", FirmName);
                messagewp = messagewp.Replace("[Outstanding]", Outstanding.TwoDigitDecimal());
                string MaskedMessagewp = messagewp;

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    WhatsAppNumber = WhatsAppNumber,
                    Mobile = MobileNumber,
                    Message = messagewp,
                    UserID = user.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, type, MaskedMessagewp);
            }
        }

        public void SendMpinMessage(string Mpin, int UserID, string MobileNumber, string WhatsAppNumber, string EmailID, string FirmName)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.ChangeMpin));

            if (setting == null)
                return;

            string type = Enum.GetName(typeof(SMSType), SMSType.ChangePin);
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", FirmName);
            string MaskedMessage = message;
            message = message.Replace("[Pin]", Mpin);
            message = message.Replace("[Mpin]", Mpin);

            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                WhatsAppNumber = WhatsAppNumber,
                Message = message,
                UserID = UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, objModel.Type, MaskedMessage);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);
            mail = mail.Replace("[Pin]", Mpin);
            mail = mail.Replace("[Mpin]", Mpin);

            if (setting.IsMail)
                MessageProcessing.SendEmail(EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", FirmName);
                string MaskedMessagewp = messagewp;
                messagewp = messagewp.Replace("[Pin]", Mpin);
                messagewp = messagewp.Replace("[Mpin]", Mpin);

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    Mobile = MobileNumber,
                    WhatsAppNumber = WhatsAppNumber,
                    Message = messagewp,
                    UserID = UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModel.Type, MaskedMessagewp);
            }
        }

        public void SendDeactivateMessage(UserMaster ObjModel)
        {
            if (ObjModel == null)
                return;

            int UserID = ObjModel.UserID;
            string MobileNumber = ObjModel.MobileNumber;
            string WhatsAppNumber = ObjModel.WhatsAppNumber;
            string EmailID = ObjModel.EmailID;
            string FirmName = ObjModel.FirmName;

            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.DeactivateAccount));

            if (setting == null)
                return;

            string type = Enum.GetName(typeof(SMSType), SMSType.DeactivateAccount);
            string message = setting.SmsMessage;

            message = message.Replace("[FirmName]", FirmName);

            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                WhatsAppNumber = WhatsAppNumber,
                Message = message,
                UserID = UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, objModel.Type, message);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);
            if (setting.IsMail)
                MessageProcessing.SendEmail(EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                messagewp = messagewp.Replace("[FirmName]", FirmName);
                SentSMS objModelwp = new SentSMS
                {
                    Mobile = MobileNumber,
                    WhatsAppNumber = WhatsAppNumber,
                    Message = messagewp,
                    UserID = UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModel.Type, messagewp);
            }
        }

        public void SendActivateMessage(UserMaster ObjModel)
        {
            if (ObjModel == null)
                return;

            int UserID = ObjModel.UserID;
            string MobileNumber = ObjModel.MobileNumber;
            string WhatsAppNumber = ObjModel.WhatsAppNumber;
            string EmailID = ObjModel.EmailID;
            string FirmName = ObjModel.FirmName;

            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.ActivateAccount));
            if (setting == null)
                return;

            string type = Enum.GetName(typeof(SMSType), SMSType.ActivateAccount);
            string message = setting.SmsMessage;

            message = message.Replace("[FirmName]", FirmName);
            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                WhatsAppNumber = WhatsAppNumber,
                Message = message,
                UserID = UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, objModel.Type, message);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);
            if (setting.IsMail)
                MessageProcessing.SendEmail(EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                messagewp = messagewp.Replace("[FirmName]", FirmName);
                SentSMS objModelwp = new SentSMS
                {
                    Mobile = MobileNumber,
                    WhatsAppNumber = WhatsAppNumber,
                    Message = messagewp,
                    UserID = UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModel.Type, messagewp);
            }
        }

        public string CheckSMS(string MobileNumber, string Message)
        {
            string type = Enum.GetName(typeof(SMSType), SMSType.Testing);
            string MaskedMessage = Message;
            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                Message = Message,
                UserID = Company.AdminUserID,
                Type = type
            };

            string Response = MessageProcessing.CheckSendSMS(objModel, objModel.Type, MaskedMessage);
            return Response;
        }

        public void EndUserTransactionSMS(TransactionMaster transactionMaste)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(transactionMaste.UserID);

            if (transactionMaste.ServiceID == (int)ServiceCode.Mobile)
            {
                return;
            }

            if (UserTypeID != (int)UserType.Retailer)
                return;

            string EndUserTransactionMessage = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.EndUserTransactionMessage), "No");

            if (!EndUserTransactionMessage.EqualsIgnoreCase("Yes"))
                return;

            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.EndUserTransactionMessage));
            if (setting == null)
                return;
            string Operator = new OperatorMasterDAL().SelectOperatorNameByOperatorID(transactionMaste.OperatorID);
            string ServiceName = new ServiceMasterDal().SelectServiceNameByServiceID(transactionMaste.ServiceID);

            string type = Enum.GetName(typeof(SMSType), SMSType.EndUserTransactionMessage);
            string message = setting.SmsMessage;
            message = message.Replace("[ConsumerNumber]", transactionMaste.ConsumerNumber);
            message = message.Replace("[Operator]", Operator);
            message = message.Replace("[TxnID]", transactionMaste.ReferenceNo?.Trim());
            message = message.Replace("[Amount]", transactionMaste.Amount.TwoDigitDecimal());
            message = message.Replace("[ServiceName]", ServiceName);

            SentSMS objModel = new SentSMS
            {
                Mobile = transactionMaste.ConsumerNumber,
                Message = message,
                UserID = Company.AdminUserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            MessageProcessing.SendSMS(objModel, objModel.Type, message);
            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[ConsumerNumber]", transactionMaste.ConsumerNumber);
                messagewp = messagewp.Replace("[Operator]", Operator);
                messagewp = messagewp.Replace("[TxnID]", transactionMaste.ReferenceNo?.Trim());
                messagewp = messagewp.Replace("[Amount]", transactionMaste.Amount.TwoDigitDecimal());
                messagewp = messagewp.Replace("[ServiceName]", ServiceName);

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    Mobile = transactionMaste.ConsumerNumber,
                    WhatsAppNumber = transactionMaste.ConsumerNumber,
                    Message = messagewp,
                    UserID = transactionMaste.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModelwp.Type, message);
            }
        }

        internal void SendBackUpMail(string FilePath)
        {
            UserMaster user = new UserDAL().SelectByIDFromCache(Company.AdminUserID);
            string Subject = UserMessage.BackupMailSubject;
            string AdminEmailID = user.EmailID;
            string mail = UserMessage.BackupMail;
            mail = mail.Replace("[FirmName]", user.FirmName);

            MessageProcessing.SendEmail(AdminEmailID, mail, Subject, FilePath);
        }

        public void SendFundTransferMessage(int SenderID, int ReceiverID, decimal Amount, int WalletID, string UTRNumber, float OldOutstanding, string FundTransactionID, int ServiceID, string OperatorName = "")
        {
            SendFundTransferMessageForSender(ReceiverID, SenderID, Amount, WalletID, UTRNumber, FundTransactionID, ServiceID, OperatorName);
            SendFundTransferMessageForReceiver(SenderID, ReceiverID, Amount, WalletID, UTRNumber, OldOutstanding, FundTransactionID, ServiceID, OperatorName);
        }

        private void SendFundTransferMessageForSender(int ReceiverID, int SenderID, decimal Amount, int WalletID, string UTRNumber, string FundTransactionID, int ServiceID, string OperatorName)
        {
            ViewModel setting = new ViewModel();
            if (ServiceID == (int)ServiceCode.FundTransfer)
            {
                setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.FundTransferMessage));
            }
            if (ServiceID == (int)ServiceCode.PaymentGateway)
            {
                setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.PGFundTransferMessage));
            }
            if (ServiceID == (int)ServiceCode.AEPS)
            {
                setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.AEPSFundTransferMessage));
            }

            if (ServiceID == (int)ServiceCode.MATM)
            {
                setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.MATMFundTransferMessage));
            }

            if (setting == null)
                return;

            UserMaster Receiver = new UserMaster();
            try
            {
                Receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UserMaster Sender = new UserMaster();
            try
            {
                Sender = new UserDAL().SelectByIDFromCache(SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UserWalletMaster SenderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(SenderID, WalletID);
            decimal OldBalance = SenderWallet.Balance + Amount;
            WalletMaster Wallet = new WalletMasterDal().SelectByID(WalletID);
            string type = Enum.GetName(typeof(SMSType), SMSType.FundTransfer);

            string message = setting.SmsMessage;

            if (setting.IsSMS)
            {
                message = setting.SmsMessage;

                message = message.Replace("[FirmName]", Sender.FirmName);
                message = message.Replace("[Receiver]", Receiver.FirmName);
                message = message.Replace("[ReceiverMobileNumber]", Receiver.MobileNumber);
                message = message.Replace("[Wallet]", Wallet.Name);
                message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
                message = message.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                message = message.Replace("[CurrentBalance]", SenderWallet.Balance.TwoDigitDecimal());
                message = message.Replace("[UTRNumber]", UTRNumber);
                message = message.Replace("[TxnID]", FundTransactionID);
                message = message.Replace("[Operator]", OperatorName);

                SentSMS objModel = new SentSMS
                {
                    Mobile = Sender.MobileNumber,
                    WhatsAppNumber = Sender.WhatsAppNumber,
                    Message = message,
                    UserID = Sender.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };

                MessageProcessing.SendSMS(objModel, objModel.Type, message);
            }

            if (setting.IsMail)
            {
                string mail = setting.EmailMessage;
                mail = mail.Replace("[FirmName]", Sender.FirmName);
                mail = mail.Replace("[Receiver]", Receiver.FirmName);
                mail = mail.Replace("[ReceiverMobileNumber]", Receiver.MobileNumber);
                mail = mail.Replace("[Wallet]", Wallet.Name);
                mail = mail.Replace("[Amount]", Amount.TwoDigitDecimal());
                mail = mail.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                mail = mail.Replace("[CurrentBalance]", SenderWallet.Balance.TwoDigitDecimal());
                mail = mail.Replace("[DateTime]", DateTime.Now.ToString());
                mail = mail.Replace("[UTRNumber]", UTRNumber);
                mail = mail.Replace("[TxnID]", FundTransactionID);
                mail = mail.Replace("[Operator]", OperatorName);

                MessageProcessing.SendEmail(Sender.EmailID, mail, "Fund Transfered to " + Receiver.FirmName);
            }

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", Sender.FirmName);
                messagewp = messagewp.Replace("[Receiver]", Receiver.FirmName);
                messagewp = messagewp.Replace("[ReceiverMobileNumber]", Receiver.MobileNumber);
                messagewp = messagewp.Replace("[Wallet]", Wallet.Name);
                messagewp = messagewp.Replace("[Amount]", Amount.TwoDigitDecimal());
                messagewp = messagewp.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[CurrentBalance]", SenderWallet.Balance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[DateTime]", DateTime.Now.ToString());
                messagewp = messagewp.Replace("[UTRNumber]", UTRNumber);
                messagewp = messagewp.Replace("[TxnID]", FundTransactionID);
                messagewp = messagewp.Replace("[Operator]", OperatorName);

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModel = new SentSMS
                {
                    Mobile = Sender.MobileNumber,
                    WhatsAppNumber = Sender.WhatsAppNumber,
                    Message = messagewp,
                    UserID = Sender.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModel, objModel.Type, message);
            }
        }

        private void SendFundTransferMessageForReceiver(int SenderID, int ReceiverID, decimal Amount, int WalletID, string UTRNumber, float OldOutstanding, string FundTransactionID, int ServiceID, string OperatorName)
        {
            ViewModel setting = new ViewModel();
            if (ServiceID == (int)ServiceCode.FundTransfer)
            {
                setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.FundReceiverMessage));
            }
            if (ServiceID == (int)ServiceCode.PaymentGateway)
            {
                setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.PGFundReceiverMessage));
            }
            if (ServiceID == (int)ServiceCode.AEPS)
            {
                setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.AEPSFundReceiverMessage));
            }

            if (ServiceID == (int)ServiceCode.MATM)
            {
                setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.MATMFundReceiverMessage));
            }

            if (setting == null)
                return;

            UserMaster Sender = new UserMaster();
            try
            {
                Sender = new UserDAL().SelectByIDFromCache(SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            UserMaster Receiver = new UserMaster();
            try
            {
                Receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ReceiverID, WalletID);
            decimal OldBalance = receiverWallet.Balance - Amount;
            WalletMaster Wallet = new WalletMasterDal().SelectByID(WalletID);
            float Outstanding = new CashCollectionMasterDal().SelectOutstandingByUserID(ReceiverID);
            string type = Enum.GetName(typeof(SMSType), SMSType.FundReceive);

            string message = setting.SmsMessage;
            if (setting.IsSMS)
            {
                message = setting.SmsMessage;
                message = message.Replace("[FirmName]", Receiver.FirmName);
                message = message.Replace("[Receiver]", Receiver.FirmName);
                message = message.Replace("[Sender]", Sender.FirmName);
                message = message.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                message = message.Replace("[Wallet]", Wallet.Name);
                message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
                message = message.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                message = message.Replace("[CurrentBalance]", receiverWallet.Balance.TwoDigitDecimal());
                message = message.Replace("[OldOutstanding]", OldOutstanding.ToString());
                message = message.Replace("[Outstanding]", Outstanding.ToString());
                message = message.Replace("[UTRNumber]", UTRNumber);
                message = message.Replace("[TxnID]", FundTransactionID);
                message = message.Replace("[Operator]", OperatorName);

                SentSMS objModel = new SentSMS
                {
                    Mobile = Receiver.MobileNumber,
                    WhatsAppNumber = Receiver.WhatsAppNumber,
                    Message = message,
                    UserID = Receiver.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendSMS(objModel, objModel.Type, message);
            }

            if (setting.IsMail)
            {
                string mail = setting.EmailMessage;
                mail = mail.Replace("[FirmName]", Receiver.FirmName);
                mail = mail.Replace("[Receiver]", Receiver.FirmName);
                mail = mail.Replace("[Sender]", Sender.FirmName);
                mail = mail.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                mail = mail.Replace("[Wallet]", Wallet.Name);
                mail = mail.Replace("[Amount]", Amount.TwoDigitDecimal());
                mail = mail.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                mail = mail.Replace("[CurrentBalance]", receiverWallet.Balance.TwoDigitDecimal());
                mail = mail.Replace("[OldOutstanding]", OldOutstanding.ToString());
                mail = mail.Replace("[Outstanding]", Outstanding.ToString());
                mail = mail.Replace("[UTRNumber]", UTRNumber);
                mail = mail.Replace("[TxnID]", FundTransactionID);
                mail = mail.Replace("[Operator]", OperatorName);

                MessageProcessing.SendEmail(Receiver.EmailID, mail, "Fund Received from " + Sender.FirmName);
            }

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", Receiver.FirmName);
                messagewp = messagewp.Replace("[Receiver]", Receiver.FirmName);
                messagewp = messagewp.Replace("[Sender]", Sender.FirmName);
                messagewp = messagewp.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                messagewp = messagewp.Replace("[Wallet]", Wallet.Name);
                messagewp = messagewp.Replace("[Amount]", Amount.TwoDigitDecimal());
                messagewp = messagewp.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[CurrentBalance]", receiverWallet.Balance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[OldOutstanding]", OldOutstanding.ToString());
                messagewp = messagewp.Replace("[Outstanding]", Outstanding.ToString());
                messagewp = messagewp.Replace("[UTRNumber]", UTRNumber);
                messagewp = messagewp.Replace("[TxnID]", FundTransactionID);
                messagewp = messagewp.Replace("[Operator]", OperatorName);

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModel = new SentSMS
                {
                    Mobile = Receiver.MobileNumber,
                    WhatsAppNumber = Receiver.WhatsAppNumber,
                    Message = messagewp,
                    UserID = Receiver.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModel, objModel.Type, message);
            }
        }

        public void SendFundRejectMessage(int SenderID, int ReceiverID, decimal Amount, int WalletID, string Remark)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.FundRejectMessage));

            UserMaster Sender = new UserMaster();
            try
            {
                Sender = new UserDAL().SelectByIDFromCache(SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            UserMaster Receiver = new UserMaster();
            try
            {
                Receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ReceiverID, WalletID);
            WalletMaster Wallet = new WalletMasterDal().SelectByID(WalletID);
            float Outstanding = new CashCollectionMasterDal().SelectOutstandingByUserID(ReceiverID);
            string type = Enum.GetName(typeof(SMSType), SMSType.FundReject);

            string message = setting.SmsMessage;
            if (setting.IsSMS)
            {
                message = setting.SmsMessage;
                message = message.Replace("[FirmName]", Receiver.FirmName);
                message = message.Replace("[Sender]", Sender.FirmName);
                message = message.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                message = message.Replace("[Wallet]", Wallet.Name);
                message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
                message = message.Replace("[CurrentBalance]", receiverWallet.Balance.TwoDigitDecimal());
                message = message.Replace("[Outstanding]", Outstanding.ToString());
                message = message.Replace("[Remark]", Remark);
                SentSMS objModel = new SentSMS
                {
                    Mobile = Receiver.MobileNumber,
                    WhatsAppNumber = Receiver.WhatsAppNumber,
                    Message = message,
                    UserID = Receiver.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendSMS(objModel, objModel.Type, message);
            }

            if (setting.IsMail)
            {
                string mail = setting.EmailMessage;
                mail = mail.Replace("[FirmName]", Receiver.FirmName);
                mail = mail.Replace("[Sender]", Sender.FirmName);
                mail = mail.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                mail = mail.Replace("[Wallet]", Wallet.Name);
                mail = mail.Replace("[Amount]", Amount.TwoDigitDecimal());
                mail = mail.Replace("[CurrentBalance]", receiverWallet.Balance.TwoDigitDecimal());
                mail = mail.Replace("[Outstanding]", Outstanding.ToString());
                mail = mail.Replace("[Remark]", Remark);
                MessageProcessing.SendEmail(Receiver.EmailID, mail, "Fund Reject from " + Sender.FirmName);
            }

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", Receiver.FirmName);
                messagewp = messagewp.Replace("[Sender]", Sender.FirmName);
                messagewp = messagewp.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                messagewp = messagewp.Replace("[Wallet]", Wallet.Name);
                messagewp = messagewp.Replace("[Amount]", Amount.TwoDigitDecimal());
                messagewp = messagewp.Replace("[CurrentBalance]", receiverWallet.Balance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[Outstanding]", Outstanding.ToString());
                messagewp = messagewp.Replace("[Remark]", Remark);
                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModel = new SentSMS
                {
                    Mobile = Receiver.MobileNumber,
                    WhatsAppNumber = Receiver.WhatsAppNumber,
                    Message = messagewp,
                    UserID = Receiver.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModel, objModel.Type, message);
            }
        }

        public void SendFundReverseMessage(int SenderID, int ReceiverID, decimal Amount, int WalletID, string UTRNumber, string AdminRemark, string FundTransactionID)
        {   // for downline
            SendFundReverseMessageForSender(ReceiverID, SenderID, Amount, WalletID, UTRNumber, AdminRemark, FundTransactionID);

            // for upline
            SendFundReverseMessageForReceiver(SenderID, ReceiverID, Amount, WalletID, UTRNumber, AdminRemark, FundTransactionID);
        }

        private void SendFundReverseMessageForSender(int ReceiverID, int SenderID, decimal Amount, int WalletID, string UTRNumber, string AdminRemark, string FundTransactionID)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.FundReverseMessage));

            UserMaster Receiver = new UserMaster();
            try
            {
                Receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UserMaster Sender = new UserMaster();
            try
            {
                Sender = new UserDAL().SelectByIDFromCache(SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            WalletMaster Wallet = new WalletMasterDal().SelectByID(WalletID);
            UserWalletMaster SenderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(SenderID, WalletID);
            decimal OldBalance = SenderWallet.Balance + Amount;
            float Outstanding = new CashCollectionMasterDal().SelectOutstandingByUserID(SenderID);
            string type = Enum.GetName(typeof(SMSType), SMSType.FundReverse);

            string message = setting.SmsMessage;
            if (setting.IsSMS)
            {
                message = setting.SmsMessage;
                message = message.Replace("[FirmName]", Sender.FirmName);
                message = message.Replace("[Receiver]", Receiver.FirmName);
                message = message.Replace("[ReceiverMobileNumber]", Receiver.MobileNumber);

                message = message.Replace("[Sender]", Sender.FirmName);
                message = message.Replace("[SenderMobileNumber]", Sender.MobileNumber);

                message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
                message = message.Replace("[Wallet]", Wallet.Name);
                message = message.Replace("[CurrentBalance]", SenderWallet.Balance.TwoDigitDecimal());
                message = message.Replace("[Outstanding]", Outstanding.ToString());
                message = message.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                message = message.Replace("[UTRNumber]", UTRNumber);
                message = message.Replace("[Remark]", AdminRemark);
                message = message.Replace("[TxnID]", FundTransactionID);
                SentSMS objModel = new SentSMS
                {
                    Mobile = Sender.MobileNumber,
                    WhatsAppNumber = Sender.WhatsAppNumber,
                    Message = message,
                    UserID = Sender.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendSMS(objModel, objModel.Type, message);
            }

            if (setting.IsMail)
            {
                string mail = setting.EmailMessage;
                mail = mail.Replace("[FirmName]", Sender.FirmName);
                mail = mail.Replace("[Receiver]", Receiver.FirmName);
                mail = mail.Replace("[ReceiverMobileNumber]", Receiver.MobileNumber);

                mail = mail.Replace("[Sender]", Sender.FirmName);
                mail = mail.Replace("[SenderMobileNumber]", Sender.MobileNumber);

                mail = mail.Replace("[Amount]", Amount.TwoDigitDecimal());
                mail = mail.Replace("[Wallet]", Wallet.Name);
                mail = mail.Replace("[CurrentBalance]", SenderWallet.Balance.TwoDigitDecimal());
                mail = mail.Replace("[Outstanding]", Outstanding.ToString());
                mail = mail.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                mail = mail.Replace("[UTRNumber]", UTRNumber);
                mail = mail.Replace("[Remark]", AdminRemark);
                mail = mail.Replace("[TxnID]", FundTransactionID);
                MessageProcessing.SendEmail(Sender.EmailID, mail, "Fund Reversed by " + Receiver.FirmName);
            }

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", Sender.FirmName);
                messagewp = messagewp.Replace("[Receiver]", Receiver.FirmName);
                messagewp = messagewp.Replace("[ReceiverMobileNumber]", Receiver.MobileNumber);

                messagewp = messagewp.Replace("[Sender]", Sender.FirmName);
                messagewp = messagewp.Replace("[SenderMobileNumber]", Sender.MobileNumber);

                messagewp = messagewp.Replace("[Amount]", Amount.TwoDigitDecimal());
                messagewp = messagewp.Replace("[Wallet]", Wallet.Name);
                messagewp = messagewp.Replace("[CurrentBalance]", SenderWallet.Balance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[Outstanding]", Outstanding.ToString());
                messagewp = messagewp.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[UTRNumber]", UTRNumber);
                messagewp = messagewp.Replace("[Remark]", AdminRemark);
                messagewp = messagewp.Replace("[TxnID]", FundTransactionID);
                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModel = new SentSMS
                {
                    Mobile = Sender.MobileNumber,
                    WhatsAppNumber = Sender.WhatsAppNumber,
                    Message = messagewp,
                    UserID = Sender.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModel, objModel.Type, message);
            }
        }

        private void SendFundReverseMessageForReceiver(int SenderID, int ReceiverID, decimal Amount, int WalletID, string UTRNumber, string AdminRemark, string FundTransactionID)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.FundReverseMessage));

            UserMaster Sender = new UserMaster();
            try
            {
                Sender = new UserDAL().SelectByIDFromCache(SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            UserMaster Receiver = new UserMaster();
            try
            {
                Receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            WalletMaster Wallet = new WalletMasterDal().SelectByID(WalletID);
            UserWalletMaster ReceiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(SenderID, WalletID);
            decimal OldBalance = ReceiverWallet.Balance - Amount;
            string type = Enum.GetName(typeof(SMSType), SMSType.FundReverse);

            string message = setting.SmsMessage;
            if (setting.IsSMS)
            {
                message = setting.SmsMessage;

                message = message.Replace("[FirmName]", Receiver.FirmName);

                message = message.Replace("[Receiver]", Receiver.FirmName);
                message = message.Replace("[ReceiverMobileNumber]", Receiver.MobileNumber);

                message = message.Replace("[Sender]", Sender.FirmName);
                message = message.Replace("[SenderMobileNumber]", Sender.MobileNumber);

                message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
                message = message.Replace("[Wallet]", Wallet.Name);
                message = message.Replace("[CurrentBalance]", ReceiverWallet.Balance.TwoDigitDecimal());
                message = message.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                message = message.Replace("[UTRNumber]", UTRNumber);
                message = message.Replace("[Remark]", AdminRemark);
                message = message.Replace("[TxnID]", FundTransactionID);
                SentSMS objModel = new SentSMS
                {
                    Mobile = Receiver.MobileNumber,
                    WhatsAppNumber = Receiver.WhatsAppNumber,
                    Message = message,
                    UserID = Receiver.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendSMS(objModel, objModel.Type, message);
            }

            if (setting.IsMail)
            {
                string mail = setting.EmailMessage;
                mail = mail.Replace("[FirmName]", Receiver.FirmName);

                mail = mail.Replace("[Receiver]", Receiver.FirmName);
                mail = mail.Replace("[ReceiverMobileNumber]", Receiver.MobileNumber);

                mail = mail.Replace("[Sender]", Sender.FirmName);
                mail = mail.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                mail = mail.Replace("[Amount]", Amount.TwoDigitDecimal());
                mail = mail.Replace("[Wallet]", Wallet.Name);
                mail = mail.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                mail = mail.Replace("[UTRNumber]", UTRNumber);
                mail = mail.Replace("[Remark]", AdminRemark);
                mail = mail.Replace("[TxnID]", FundTransactionID);
                MessageProcessing.SendEmail(Receiver.EmailID, mail, "Fund reversed from " + Sender.FirmName);
            }

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", Receiver.FirmName);

                messagewp = messagewp.Replace("[Receiver]", Receiver.FirmName);
                messagewp = messagewp.Replace("[ReceiverMobileNumber]", Receiver.MobileNumber);

                messagewp = messagewp.Replace("[Sender]", Sender.FirmName);
                messagewp = messagewp.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                messagewp = messagewp.Replace("[Amount]", Amount.TwoDigitDecimal());
                messagewp = messagewp.Replace("[Wallet]", Wallet.Name);
                messagewp = messagewp.Replace("[CurrentBalance]", ReceiverWallet.Balance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[UTRNumber]", UTRNumber);
                messagewp = messagewp.Replace("[Remark]", AdminRemark);
                messagewp = messagewp.Replace("[TxnID]", FundTransactionID);
                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModel = new SentSMS
                {
                    Mobile = Receiver.MobileNumber,
                    WhatsAppNumber = Receiver.WhatsAppNumber,
                    Message = messagewp,
                    UserID = Receiver.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModel, objModel.Type, message);
            }
        }

        public void Send2FAOTP(UserMaster user)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.TwoFactorAuthentication));
            string type = Enum.GetName(typeof(SMSType), SMSType.TwoFactorAuthentication);
            OTPCore(user, setting, type);
        }

        internal void SendComplaintAdminMessage(int UserID, string ConsumerNumber, decimal Amount, string OperatorName)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.ComplaintAdd));

            if (setting == null)
                return;
            UserMaster admin = new UserMaster();
            try
            {
                admin = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            string type = Enum.GetName(typeof(SMSType), SMSType.Complaint);
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", admin.FirmName);
            message = message.Replace("[ConsumerNumber]", ConsumerNumber);
            message = message.Replace("[Operator]", OperatorName);
            message = message.Replace("[Amount]", Amount.TwoDigitDecimal());

            SentSMS objModel = new SentSMS
            {
                Mobile = admin.MobileNumber,
                Message = message,
                UserID = UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, objModel.Type);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", admin.FirmName);
            mail = mail.Replace("[ConsumerNumber]", ConsumerNumber);
            mail = mail.Replace("[Operator]", OperatorName);
            mail = mail.Replace("[Amount]", Amount.TwoDigitDecimal());

            if (setting.IsMail)
                MessageProcessing.SendEmail(admin.EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", admin.FirmName);
                messagewp = messagewp.Replace("[ConsumerNumber]", ConsumerNumber);
                messagewp = messagewp.Replace("[Operator]", OperatorName);
                messagewp = messagewp.Replace("[Amount]", Amount.TwoDigitDecimal());

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    Mobile = admin.MobileNumber,
                    WhatsAppNumber = admin.WhatsAppNumber,
                    Message = messagewp,
                    UserID = UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModel.Type);
            }
        }

        internal void SendComplaintAcceptMessage(int UserID, string ConsumerNumber, decimal Amount, decimal balance, string OperatorName)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.ComplaintAccept));

            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            string type = Enum.GetName(typeof(SMSType), SMSType.Complaint);
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", user.FirmName);
            message = message.Replace("[ConsumerNumber]", ConsumerNumber);
            message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
            message = message.Replace("[CurrentBalance]", balance.TwoDigitDecimal());
            message = message.Replace("[Operator]", OperatorName);

            SentSMS objModel = new SentSMS
            {
                Mobile = user.MobileNumber,
                WhatsAppNumber = user.WhatsAppNumber,
                Message = message,
                UserID = UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, objModel.Type);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", user.FirmName);
            mail = mail.Replace("[ConsumerNumber]", ConsumerNumber);
            mail = mail.Replace("[Amount]", Amount.TwoDigitDecimal());
            mail = mail.Replace("[CurrentBalance]", balance.TwoDigitDecimal());
            mail = mail.Replace("[Operator]", OperatorName);

            if (setting.IsMail)
                MessageProcessing.SendEmail(user.EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", user.FirmName);
                messagewp = messagewp.Replace("[ConsumerNumber]", ConsumerNumber);
                messagewp = messagewp.Replace("[Amount]", Amount.TwoDigitDecimal());
                messagewp = messagewp.Replace("[CurrentBalance]", balance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[Operator]", OperatorName);

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    WhatsAppNumber = user.WhatsAppNumber,
                    Mobile = user.MobileNumber,
                    Message = messagewp,
                    UserID = UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };

                MessageProcessing.SendWhatsAppSMS(objModelwp, objModel.Type);
            }
        }

        internal void SendComplaintRejectMessage(int UserID, string ConsumerNumber, decimal Amount, decimal balance, string OperatorName)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.ComplaintReject));

            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            string type = Enum.GetName(typeof(SMSType), SMSType.Complaint);
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", user.FirmName);
            message = message.Replace("[ConsumerNumber]", ConsumerNumber);
            message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
            message = message.Replace("[CurrentBalance]", balance.TwoDigitDecimal());
            message = message.Replace("[Operator]", OperatorName);

            SentSMS objModel = new SentSMS
            {
                Mobile = user.MobileNumber,
                WhatsAppNumber = user.WhatsAppNumber,
                Message = message,
                UserID = UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, objModel.Type);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", user.FirmName);
            mail = mail.Replace("[ConsumerNumber]", ConsumerNumber);
            mail = mail.Replace("[Amount]", Amount.TwoDigitDecimal());
            mail = mail.Replace("[Operator]", OperatorName);
            mail = mail.Replace("[CurrentBalance]", balance.TwoDigitDecimal());

            if (setting.IsMail)
                MessageProcessing.SendEmail(user.EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", user.FirmName);
                messagewp = messagewp.Replace("[ConsumerNumber]", ConsumerNumber);
                messagewp = messagewp.Replace("[Amount]", Amount.TwoDigitDecimal());
                messagewp = messagewp.Replace("[CurrentBalance]", balance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[Operator]", OperatorName);

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    WhatsAppNumber = user.WhatsAppNumber,
                    Mobile = user.MobileNumber,
                    Message = messagewp,
                    UserID = UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };

                MessageProcessing.SendWhatsAppSMS(objModelwp, objModel.Type);
            }
        }

        internal void SendDMTOfflineMessage()
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.DMTOfflineMessage));

            if (setting == null)
                return;
            UserMaster admin = new UserMaster();
            try
            {
                admin = new UserDAL().SelectByIDFromCache(Company.AdminUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            string type = Enum.GetName(typeof(SMSType), SMSType.DMTOfflineMessage);
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", admin.FirmName);

            SentSMS objModel = new SentSMS
            {
                Mobile = admin.MobileNumber,
                Message = message,
                UserID = Company.AdminUserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, objModel.Type);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", admin.FirmName);

            if (setting.IsMail)
                MessageProcessing.SendEmail(admin.EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", admin.FirmName);

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    Mobile = admin.MobileNumber,
                    WhatsAppNumber = admin.WhatsAppNumber,
                    Message = messagewp,
                    UserID = Company.AdminUserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModel.Type);
            }
        }

        internal void SendCashCollectionMessage(int SenderID, int ReceiverID, decimal Amount, decimal OldCredit, decimal TodaysCredit, string referenceID)
        {
            if (Amount <= 0.00001M)
            {
                return;
            }

            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.CashCollection));

            UserMaster Sender = new UserMaster();
            try
            {
                Sender = new UserDAL().SelectByIDFromCache(SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }

            UserMaster Receiver = new UserMaster();
            try
            {
                Receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }

            string type = Enum.GetName(typeof(SMSType), SMSType.CashCollection);
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", Receiver.FirmName);
            message = message.Replace("[Sender]", Sender.FirmName);
            message = message.Replace("[SystemName]", Company.ClientName);
            message = message.Replace("[OldOutstanding]", OldCredit.TwoDigitDecimal());
            message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
            message = message.Replace("[Outstanding]", TodaysCredit.TwoDigitDecimal());
            message = message.Replace("[TxnID]", referenceID);

            SentSMS objModel = new SentSMS
            {
                Mobile = Receiver.MobileNumber,
                WhatsAppNumber = Receiver.WhatsAppNumber,
                Message = message,
                UserID = Company.AdminUserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, objModel.Type);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", Receiver.FirmName);
            mail = mail.Replace("[Sender]", Sender.FirmName);
            mail = mail.Replace("[SystemName]", Company.ClientName);
            mail = mail.Replace("[OldOutstanding]", OldCredit.TwoDigitDecimal());
            mail = mail.Replace("[Amount]", Amount.TwoDigitDecimal());
            mail = mail.Replace("[Outstanding]", TodaysCredit.TwoDigitDecimal());
            mail = mail.Replace("[TxnID]", referenceID);

            if (setting.IsMail)
                MessageProcessing.SendEmail(Receiver.EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", Receiver.FirmName);
                messagewp = messagewp.Replace("[Sender]", Sender.FirmName);
                messagewp = messagewp.Replace("[SystemName]", Company.ClientName);
                messagewp = messagewp.Replace("[OldOutstanding]", OldCredit.TwoDigitDecimal());
                messagewp = messagewp.Replace("[Amount]", Amount.TwoDigitDecimal());
                messagewp = messagewp.Replace("[Outstanding]", TodaysCredit.TwoDigitDecimal());
                messagewp = messagewp.Replace("[TxnID]", referenceID);

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    WhatsAppNumber = Receiver.WhatsAppNumber,
                    Mobile = Receiver.MobileNumber,
                    Message = messagewp,
                    UserID = Company.AdminUserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };

                MessageProcessing.SendWhatsAppSMS(objModelwp, objModel.Type);
            }
        }

        public void SendWalletTranferMessage(int ReceiverID, int SenderID, decimal Amount, int WalletID, string fromWalletName, string toWalletName)
        {
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.WalletTranferMessage));

            UserMaster Sender = new UserMaster();
            try
            {
                Sender = new UserDAL().SelectByIDFromCache(SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            UserMaster Receiver = new UserMaster();
            try
            {
                Receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ReceiverID, WalletID);
            decimal OldBalance = receiverWallet.Balance - Amount;
            float Outstanding = new CashCollectionMasterDal().SelectOutstandingByUserID(ReceiverID);
            string type = Enum.GetName(typeof(SMSType), SMSType.WalletTranfer);

            string message = setting.SmsMessage;
            if (setting.IsSMS)
            {
                message = setting.SmsMessage;
                message = message.Replace("[FirmName]", Receiver.FirmName);
                message = message.Replace("[Sender]", Sender.FirmName);
                message = message.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                message = message.Replace("[Wallet]", fromWalletName);
                message = message.Replace("[ToWallet]", toWalletName);
                message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
                message = message.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                message = message.Replace("[CurrentBalance]", receiverWallet.Balance.TwoDigitDecimal());

                SentSMS objModel = new SentSMS
                {
                    Mobile = Receiver.MobileNumber,
                    WhatsAppNumber = Receiver.WhatsAppNumber,
                    Message = message,
                    UserID = Receiver.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendSMS(objModel, objModel.Type, message);
            }

            if (setting.IsMail)
            {
                string mail = setting.EmailMessage;
                mail = mail.Replace("[FirmName]", Receiver.FirmName);
                mail = mail.Replace("[Sender]", Sender.FirmName);
                mail = mail.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                mail = mail.Replace("[Wallet]", fromWalletName);
                mail = mail.Replace("[ToWallet]", toWalletName);
                mail = mail.Replace("[Amount]", Amount.TwoDigitDecimal());
                mail = mail.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                mail = mail.Replace("[CurrentBalance]", receiverWallet.Balance.TwoDigitDecimal());

                MessageProcessing.SendEmail(Receiver.EmailID, mail, "Fund Received from " + Sender.FirmName);
            }

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", Receiver.FirmName);
                messagewp = messagewp.Replace("[Sender]", Sender.FirmName);
                messagewp = messagewp.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                messagewp = messagewp.Replace("[Wallet]", fromWalletName);
                messagewp = messagewp.Replace("[ToWallet]", toWalletName);
                messagewp = messagewp.Replace("[Amount]", Amount.TwoDigitDecimal());
                messagewp = messagewp.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[CurrentBalance]", receiverWallet.Balance.TwoDigitDecimal());

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModel = new SentSMS
                {
                    Mobile = Receiver.MobileNumber,
                    WhatsAppNumber = Receiver.WhatsAppNumber,
                    Message = messagewp,
                    UserID = Receiver.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModel, objModel.Type, message);
            }
        }

        internal void SendMorningMessage(UserMaster user, decimal Outstanding, decimal Balance, decimal DMTBalance)
        {
            if (user == null) return;

            string type = Enum.GetName(typeof(SMSType), SMSType.MorningMessage);
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.MorningMessage));

            string MobileNumber = user.MobileNumber;
            string WhatsAppNumber = user.WhatsAppNumber;
            string FirmName = user.FirmName;
            string EmailID = user.EmailID;

            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", FirmName);
            message = message.Replace("[Outstanding]", Outstanding.TwoDigitDecimal());
            message = message.Replace("[CurrentBalance]", Balance.TwoDigitDecimal());
            message = message.Replace("[DMTBalance]", DMTBalance.TwoDigitDecimal());
            string MaskedMessage = message;
            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                WhatsAppNumber = WhatsAppNumber,
                Message = message,
                UserID = user.UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, type, MaskedMessage);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);
            mail = mail.Replace("[Outstanding]", Outstanding.TwoDigitDecimal());
            mail = mail.Replace("[CurrentBalance]", Balance.TwoDigitDecimal());
            mail = mail.Replace("[DMTBalance]", DMTBalance.TwoDigitDecimal());
            if (setting.IsMail)
                MessageProcessing.SendEmail(EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", FirmName);
                messagewp = messagewp.Replace("[Outstanding]", Outstanding.TwoDigitDecimal());
                messagewp = messagewp.Replace("[CurrentBalance]", Balance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[DMTBalance]", DMTBalance.TwoDigitDecimal());
                string MaskedMessagewp = messagewp;

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    WhatsAppNumber = WhatsAppNumber,
                    Mobile = MobileNumber,
                    Message = messagewp,
                    UserID = user.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, type, MaskedMessagewp);
            }
        }

        internal void SendProviderLowBalanceMessage(int ProviderID, decimal Balance)
        {
            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(Company.AdminUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            string Provider = new ProviderMasterDAL().SelectProviderNameByProviderID(ProviderID);

            string type = Enum.GetName(typeof(SMSType), SMSType.ProviderLowBalance);
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.ProviderLowBalance));

            string MobileNumber = user.MobileNumber;
            string WhatsAppNumber = user.WhatsAppNumber;
            string FirmName = user.FirmName;
            string EmailID = user.EmailID;

            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", FirmName);
            message = message.Replace("[Provider]", Provider);
            message = message.Replace("[CurrentBalance]", Balance.TwoDigitDecimal());
            string MaskedMessage = message;
            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                WhatsAppNumber = WhatsAppNumber,
                Message = message,
                UserID = user.UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, type, MaskedMessage);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);
            mail = mail.Replace("[Provider]", Provider);
            mail = mail.Replace("[CurrentBalance]", Balance.TwoDigitDecimal());
            if (setting.IsMail)
                MessageProcessing.SendEmail(EmailID, mail, type);

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", FirmName);
                messagewp = messagewp.Replace("[Provider]", Provider);
                messagewp = messagewp.Replace("[CurrentBalance]", Balance.TwoDigitDecimal());
                string MaskedMessagewp = messagewp;

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    WhatsAppNumber = WhatsAppNumber,
                    Mobile = MobileNumber,
                    Message = messagewp,
                    UserID = user.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, type, MaskedMessagewp);
            }
        }

        public void SendFundTransferOTP(UserMaster user)
        {
            if (user == null)
                return;
            string type = Enum.GetName(typeof(SMSType), SMSType.FundTransferOTP);
            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.FundTransferOTP));
            OTPCore(user, setting, type);
        }

        private void OTPCore(UserMaster user, ViewModel setting, string Type)
        {
            string OTP = "";
            string EncOTP = OTPProcessing.GenerateOTP(out OTP);
            new OTPMasterDal().Insert(new OTPMaster
            {
                OTP = EncOTP,
                UserID = user.UserID,
                Type = Type
            });

            string MobileNumber = user.MobileNumber;
            string WhatsAppNumber = user.WhatsAppNumber;
            string FirmName = user.FirmName;
            string EmailID = user.EmailID;

            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", FirmName);
            string MaskedMessage = message;
            message = message.Replace("[OTP]", OTP);

            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                WhatsAppNumber = WhatsAppNumber,
                Message = message,
                UserID = user.UserID,
                Type = Type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS) MessageProcessing.SendSMS(objModel, Type, MaskedMessage);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);
            mail = mail.Replace("[OTP]", OTP);
            if (setting.IsMail)
                MessageProcessing.SendEmail(EmailID, mail, Type);

            if (setting.IsWhatsUp)
            {
                string wpmessage = setting.WhatsUpMessage;
                wpmessage = wpmessage.Replace("[FirmName]", FirmName);
                string MaskedMessagewp = wpmessage;
                wpmessage = wpmessage.Replace("[OTP]", OTP);

                if (wpmessage.Length <= 5)
                {
                    wpmessage = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    Mobile = MobileNumber,
                    WhatsAppNumber = WhatsAppNumber,
                    Message = wpmessage,
                    UserID = user.UserID,
                    Type = Type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, Type, MaskedMessagewp);
            }

            if (setting.ISOTP) MessageProcessing.SendSMSWITHOTP(objModel, Type, MaskedMessage);


        }

        private int SignUpOTPCore(string MobileNumber, ViewModel setting, string Type)
        {
            string OTP = "";
            int OTPMasterID = 0;
            string EncOTP = OTPProcessing.GenerateOTP(out OTP);
            OTPMasterID = new OTPMasterDal().Insert(new OTPMaster
            {
                OTP = EncOTP,
                Type = Type,
                MobileNumber = MobileNumber
            });

            string message = setting.SmsMessage;
            //message = message.Replace("[FirmName]", FirmName);

            message = message.Replace("[OTP]", OTP);
            string MaskedMessage = message;

            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                WhatsAppNumber = MobileNumber,
                Message = message,
                Type = Type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, Type, MaskedMessage);


            if (setting.IsWhatsUp)
            {
                string wpmessage = setting.WhatsUpMessage;
                //wpmessage = wpmessage.Replace("[FirmName]", FirmName);
                string MaskedMessagewp = wpmessage;
                wpmessage = wpmessage.Replace("[OTP]", OTP);

                if (wpmessage.Length <= 5)
                {
                    wpmessage = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    Mobile = MobileNumber,
                    WhatsAppNumber = MobileNumber,
                    Message = wpmessage,
                    Type = Type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, Type, MaskedMessagewp);
            }
            return OTPMasterID;
        }

        internal void SendPayoutTransactionMessage(int userID, string status, TransactionMaster transactionMaster)
        {
            if (status.EqualsIgnoreCase(StatusType.Failure))
                return;

            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (user.UserTypeID == (int)UserType.APIUser)
                return;

            ViewModel setting = new ViewModel();

            if (status.EqualsIgnoreCase(StatusType.Success))
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.PayoutTransactionSuccess));

            if (status.EqualsIgnoreCase(StatusType.Process))
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.PayoutTransactionProcess));

            if (status.EqualsIgnoreCase(StatusType.Failure))
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.PayoutTransactionFail));

            string type = "Payout Message";
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", user.FirmName);
            message = message.Replace("[Status]", transactionMaster.Status);
            message = message.Replace("[AccountNo]", transactionMaster.ConsumerNumber);
            message = message.Replace("[TxnID]", transactionMaster.ReferenceNo);
            message = message.Replace("[Amount]", transactionMaster.Amount.TwoDigitDecimal());
            message = message.Replace("[Name]", transactionMaster.DMT_BeneficaryNameByBank);
            SentSMS objModel = new SentSMS
            {
                Mobile = user.MobileNumber,
                Message = message,
                UserID = userID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, type);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", user.FirmName);
            mail = mail.Replace("[Status]", transactionMaster.Status);
            mail = mail.Replace("[AccountNo]", transactionMaster.ConsumerNumber);
            mail = mail.Replace("[TxnID]", transactionMaster.ReferenceNo);
            mail = mail.Replace("[Amount]", transactionMaster.Amount.TwoDigitDecimal());
            mail = mail.Replace("[Name]", transactionMaster.DMT_BeneficaryNameByBank);

            if (setting.IsMail)
                MessageProcessing.SendEmail(user.EmailID, mail, type);

            string messagewp = setting.WhatsUpMessage;
            messagewp = messagewp.Replace("[FirmName]", user.FirmName);
            messagewp = messagewp.Replace("[Status]", transactionMaster.Status);
            messagewp = messagewp.Replace("[AccountNo]", transactionMaster.ConsumerNumber);
            messagewp = messagewp.Replace("[TxnID]", transactionMaster.ReferenceNo);
            messagewp = messagewp.Replace("[Amount]", transactionMaster.Amount.TwoDigitDecimal());
            messagewp = messagewp.Replace("[Name]", transactionMaster.DMT_BeneficaryNameByBank);

            if (messagewp.Length <= 5)
            {
                messagewp = message;
            }

            SentSMS objModelwp = new SentSMS
            {
                WhatsAppNumber = user.WhatsAppNumber,
                Mobile = user.MobileNumber,
                Message = messagewp,
                UserID = userID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsWhatsUp)
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModelwp.Type);
        }

        internal void SendOTTTransactionMessage(int userID, string status, TransactionMaster transactionMaster)
        {
            if (status.EqualsIgnoreCase(StatusType.Failure))
                return;

            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (user.UserTypeID == (int)UserType.APIUser)
                return;

            ViewModel setting = new ViewModel();

            if (status.EqualsIgnoreCase(StatusType.Success))
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.OTTTransactionSuccess));

            if (status.EqualsIgnoreCase(StatusType.Process))
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.OTTTransactionProcess));

            if (status.EqualsIgnoreCase(StatusType.Failure))
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.OTTTransactionFail));

            string type = "OTT Message";
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", user.FirmName);
            message = message.Replace("[Status]", transactionMaster.Status);
            message = message.Replace("[ConsumerNumber]", transactionMaster.ConsumerNumber);
            message = message.Replace("[TxnID]", transactionMaster.ReferenceNo);
            message = message.Replace("[Amount]", transactionMaster.Amount.TwoDigitDecimal());

            SentSMS objModel = new SentSMS
            {
                Mobile = user.MobileNumber,
                Message = message,
                UserID = userID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, type);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", user.FirmName);
            mail = mail.Replace("[Status]", transactionMaster.Status);
            mail = mail.Replace("[ConsumerNumber]", transactionMaster.ConsumerNumber);
            mail = mail.Replace("[TxnID]", transactionMaster.ReferenceNo);
            mail = mail.Replace("[Amount]", transactionMaster.Amount.TwoDigitDecimal());

            if (setting.IsMail)
                MessageProcessing.SendEmail(user.EmailID, mail, type);

            string messagewp = setting.WhatsUpMessage;
            messagewp = messagewp.Replace("[FirmName]", user.FirmName);
            messagewp = messagewp.Replace("[Status]", transactionMaster.Status);
            messagewp = messagewp.Replace("[ConsumerNumber]", transactionMaster.ConsumerNumber);
            messagewp = messagewp.Replace("[TxnID]", transactionMaster.ReferenceNo);
            messagewp = messagewp.Replace("[Amount]", transactionMaster.Amount.TwoDigitDecimal());

            if (messagewp.Length <= 5)
            {
                messagewp = message;
            }

            SentSMS objModelwp = new SentSMS
            {
                WhatsAppNumber = user.WhatsAppNumber,
                Mobile = user.MobileNumber,
                Message = messagewp,
                UserID = userID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsWhatsUp)
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModelwp.Type);
        }

        internal void SendEnquiryMessage(string FirmName, string MobileNumber, string EmailID, bool IsSubmit = false)
        {
            ViewModel setting = new ViewModel();
            if (!IsSubmit)
            {
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.EnquiryAction));
            }
            else
            {
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.EnquirySenderMessage));
                SendEnquiryAdminMessage();
            }

            string type = "Enquiry Sender";
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", FirmName);

            SentSMS objModel = new SentSMS
            {
                Mobile = MobileNumber,
                Message = message,
                UserID = 0,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, type);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", FirmName);

            if (setting.IsMail)
                MessageProcessing.SendEmail(EmailID, mail, type);

            string messagewp = setting.WhatsUpMessage;
            messagewp = messagewp.Replace("[FirmName]", FirmName);

            if (messagewp.Length <= 5)
            {
                messagewp = message;
            }

            SentSMS objModelwp = new SentSMS
            {
                WhatsAppNumber = MobileNumber,
                Mobile = MobileNumber,
                Message = messagewp,
                UserID = 0,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsWhatsUp)
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModelwp.Type);
        }

        private void SendEnquiryAdminMessage()
        {
            UserMaster admin = new UserMaster();
            try
            {
                admin = new UserDAL().SelectByIDFromCache(Company.AdminUserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            ViewModel setting = new ViewModel();
            setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.EnquiryReceiverMessage));
            string type = "Enquiry Receiver";
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", admin.FirmName);

            SentSMS objModel = new SentSMS
            {
                Mobile = admin.MobileNumber,
                Message = message,
                UserID = admin.UserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, type);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", admin.FirmName);

            if (setting.IsMail)
                MessageProcessing.SendEmail(admin.EmailID, mail, type);

            string messagewp = setting.WhatsUpMessage;
            messagewp = messagewp.Replace("[FirmName]", admin.FirmName);

            if (messagewp.Length <= 5)
            {
                messagewp = message;
            }

            SentSMS objModelwp = new SentSMS
            {
                WhatsAppNumber = admin.MobileNumber,
                Mobile = admin.MobileNumber,
                Message = messagewp,
                UserID = 0,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsWhatsUp)
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModelwp.Type);
        }

        internal void SendUPITransactionMessage(int userID, string status, TransactionMaster transactionMaster)
        {
            if (status.EqualsIgnoreCase(StatusType.Failure))
                return;

            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (user.UserTypeID == (int)UserType.APIUser)
                return;

            ViewModel setting = new ViewModel();

            if (status.EqualsIgnoreCase(StatusType.Success))
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.UPITransactionSuccess));

            if (status.EqualsIgnoreCase(StatusType.Process))
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.UPITransactionProcess));

            if (status.EqualsIgnoreCase(StatusType.Failure))
                setting = new MessageSelection().GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.UPITransactionFail));

            string type = "UPI Message";
            string message = setting.SmsMessage;
            message = message.Replace("[FirmName]", user.FirmName);
            message = message.Replace("[Status]", transactionMaster.Status);
            message = message.Replace("[AccountNo]", transactionMaster.ConsumerNumber);
            message = message.Replace("[TxnID]", transactionMaster.ReferenceNo);
            message = message.Replace("[Amount]", transactionMaster.Amount.TwoDigitDecimal());
            message = message.Replace("[Name]", transactionMaster.DMT_BeneficaryNameByBank);
            SentSMS objModel = new SentSMS
            {
                Mobile = user.MobileNumber,
                Message = message,
                UserID = userID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, type);

            string mail = setting.EmailMessage;
            mail = mail.Replace("[FirmName]", user.FirmName);
            mail = mail.Replace("[Status]", transactionMaster.Status);
            mail = mail.Replace("[AccountNo]", transactionMaster.ConsumerNumber);
            mail = mail.Replace("[TxnID]", transactionMaster.ReferenceNo);
            mail = mail.Replace("[Amount]", transactionMaster.Amount.TwoDigitDecimal());
            mail = mail.Replace("[Name]", transactionMaster.DMT_BeneficaryNameByBank);

            if (setting.IsMail)
                MessageProcessing.SendEmail(user.EmailID, mail, type);

            string messagewp = setting.WhatsUpMessage;
            messagewp = messagewp.Replace("[FirmName]", user.FirmName);
            messagewp = messagewp.Replace("[Status]", transactionMaster.Status);
            messagewp = messagewp.Replace("[AccountNo]", transactionMaster.ConsumerNumber);
            messagewp = messagewp.Replace("[TxnID]", transactionMaster.ReferenceNo);
            messagewp = messagewp.Replace("[Amount]", transactionMaster.Amount.TwoDigitDecimal());
            messagewp = messagewp.Replace("[Name]", transactionMaster.DMT_BeneficaryNameByBank);

            if (messagewp.Length <= 5)
            {
                messagewp = message;
            }

            SentSMS objModelwp = new SentSMS
            {
                WhatsAppNumber = user.WhatsAppNumber,
                Mobile = user.MobileNumber,
                Message = messagewp,
                UserID = userID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };

            if (setting.IsWhatsUp)
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModelwp.Type);
        }

        public void EndUserRemainderMessage(string ConsumerNumber, string Operator, string Date)
        {
            string EndUserTransactionMessage = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.EndUserRemainderMessage), "No");

            if (!EndUserTransactionMessage.EqualsIgnoreCase("Yes"))
                return;

            ViewModel setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.EndUserTransactionMessage));
            if (setting == null)
                return;

            string type = Enum.GetName(typeof(SMSType), SMSType.EndUserTransactionMessage);
            string message = setting.SmsMessage;
            message = message.Replace("[ConsumerNumber]", ConsumerNumber);
            message = message.Replace("[Operator]", Operator);
            message = message.Replace("[DateTime]", Date);

            SentSMS objModel = new SentSMS
            {
                Mobile = ConsumerNumber,
                Message = message,
                UserID = Company.AdminUserID,
                Type = type,
                TemplateID = setting?.TemplateID ?? "",
            };
            if (setting.IsSMS)
                MessageProcessing.SendSMS(objModel, objModel.Type, message);
            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[ConsumerNumber]", ConsumerNumber);
                messagewp = messagewp.Replace("[Operator]", Operator);
                messagewp = messagewp.Replace("[DateTime]", Date);

                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModelwp = new SentSMS
                {
                    Mobile = ConsumerNumber,
                    Message = message,
                    UserID = Company.AdminUserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModelwp, objModelwp.Type, message);
            }
        }


        public void SendSubscriptionMessage(int SenderID, int ReceiverID, decimal Amount, int WalletID, string RefrenceID)
        {
            SendSubscriptionPaymentMessageForSender(ReceiverID, SenderID, Amount, WalletID, RefrenceID);
            SendSubscriptionPaymentMessageForReceiver(ReceiverID, SenderID, Amount, WalletID, RefrenceID);
        }

        private void SendSubscriptionPaymentMessageForSender(int ReceiverID, int SenderID, decimal Amount, int WalletID, string RefrenceID)
        {
            ViewModel setting = new ViewModel();

            setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.SubscriptionChargePayment));


            if (setting == null)
                return;

            UserMaster Receiver = new UserMaster();
            try
            {
                Receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UserMaster Sender = new UserMaster();
            try
            {
                Sender = new UserDAL().SelectByIDFromCache(SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UserWalletMaster SenderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(SenderID, WalletID);
            decimal OldBalance = SenderWallet.Balance + Amount;
            WalletMaster Wallet = new WalletMasterDal().SelectByID(WalletID);
            string type = Enum.GetName(typeof(SMSType), SMSType.SubscriptionPayment);

            string message = setting.SmsMessage;

            if (setting.IsSMS)
            {
                message = setting.SmsMessage;

                message = message.Replace("[FirmName]", Sender.FirmName);
                message = message.Replace("[Receiver]", Receiver.FirmName);
                message = message.Replace("[ReceiverMobileNumber]", Receiver.MobileNumber);
                message = message.Replace("[Wallet]", Wallet.Name);
                message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
                message = message.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                message = message.Replace("[CurrentBalance]", SenderWallet.Balance.TwoDigitDecimal());


                SentSMS objModel = new SentSMS
                {
                    Mobile = Sender.MobileNumber,
                    WhatsAppNumber = Sender.WhatsAppNumber,
                    Message = message,
                    UserID = Sender.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };

                MessageProcessing.SendSMS(objModel, objModel.Type, message);
            }

            if (setting.IsMail)
            {
                string mail = setting.EmailMessage;
                mail = mail.Replace("[FirmName]", Sender.FirmName);
                mail = mail.Replace("[Receiver]", Receiver.FirmName);
                mail = mail.Replace("[ReceiverMobileNumber]", Receiver.MobileNumber);
                mail = mail.Replace("[Wallet]", Wallet.Name);
                mail = mail.Replace("[Amount]", Amount.TwoDigitDecimal());
                mail = mail.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                mail = mail.Replace("[CurrentBalance]", SenderWallet.Balance.TwoDigitDecimal());
                mail = mail.Replace("[DateTime]", DateTime.Now.ToString());


                MessageProcessing.SendEmail(Sender.EmailID, mail, "Fund Transfered to " + Receiver.FirmName);
            }

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", Sender.FirmName);
                messagewp = messagewp.Replace("[Receiver]", Receiver.FirmName);
                messagewp = messagewp.Replace("[ReceiverMobileNumber]", Receiver.MobileNumber);
                messagewp = messagewp.Replace("[Wallet]", Wallet.Name);
                messagewp = messagewp.Replace("[Amount]", Amount.TwoDigitDecimal());
                messagewp = messagewp.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[CurrentBalance]", SenderWallet.Balance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[DateTime]", DateTime.Now.ToString());


                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModel = new SentSMS
                {
                    Mobile = Sender.MobileNumber,
                    WhatsAppNumber = Sender.WhatsAppNumber,
                    Message = messagewp,
                    UserID = Sender.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModel, objModel.Type, message);
            }
        }

        private void SendSubscriptionPaymentMessageForReceiver(int ReceiverID, int SenderID, decimal Amount, int WalletID, string RefrenceID)
        {
            ViewModel setting = new ViewModel();

            setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.SubscriptionChargePaymentReceived));


            if (setting == null)
                return;

            UserMaster Sender = new UserMaster();
            try
            {
                Sender = new UserDAL().SelectByIDFromCache(SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            UserMaster Receiver = new UserMaster();
            try
            {
                Receiver = new UserDAL().SelectByIDFromCache(ReceiverID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ReceiverID, WalletID);
            decimal OldBalance = receiverWallet.Balance - Amount;
            WalletMaster Wallet = new WalletMasterDal().SelectByID(WalletID);
            float Outstanding = new CashCollectionMasterDal().SelectOutstandingByUserID(ReceiverID);
            string type = Enum.GetName(typeof(SMSType), SMSType.SubscriptionPayment);

            string message = setting.SmsMessage;
            if (setting.IsSMS)
            {
                message = setting.SmsMessage;
                message = message.Replace("[FirmName]", Receiver.FirmName);
                message = message.Replace("[Receiver]", Receiver.FirmName);
                message = message.Replace("[Sender]", Sender.FirmName);
                message = message.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                message = message.Replace("[Wallet]", Wallet.Name);
                message = message.Replace("[Amount]", Amount.TwoDigitDecimal());
                message = message.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                message = message.Replace("[CurrentBalance]", receiverWallet.Balance.TwoDigitDecimal());


                SentSMS objModel = new SentSMS
                {
                    Mobile = Receiver.MobileNumber,
                    WhatsAppNumber = Receiver.WhatsAppNumber,
                    Message = message,
                    UserID = Receiver.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendSMS(objModel, objModel.Type, message);
            }

            if (setting.IsMail)
            {
                string mail = setting.EmailMessage;
                mail = mail.Replace("[FirmName]", Receiver.FirmName);
                mail = mail.Replace("[Receiver]", Receiver.FirmName);
                mail = mail.Replace("[Sender]", Sender.FirmName);
                mail = mail.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                mail = mail.Replace("[Wallet]", Wallet.Name);
                mail = mail.Replace("[Amount]", Amount.TwoDigitDecimal());
                mail = mail.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                mail = mail.Replace("[CurrentBalance]", receiverWallet.Balance.TwoDigitDecimal());


                MessageProcessing.SendEmail(Receiver.EmailID, mail, "Fund Received from " + Sender.FirmName);
            }

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[FirmName]", Receiver.FirmName);
                messagewp = messagewp.Replace("[Receiver]", Receiver.FirmName);
                messagewp = messagewp.Replace("[Sender]", Sender.FirmName);
                messagewp = messagewp.Replace("[SenderMobileNumber]", Sender.MobileNumber);
                messagewp = messagewp.Replace("[Wallet]", Wallet.Name);
                messagewp = messagewp.Replace("[Amount]", Amount.TwoDigitDecimal());
                messagewp = messagewp.Replace("[OldBalance]", OldBalance.TwoDigitDecimal());
                messagewp = messagewp.Replace("[CurrentBalance]", receiverWallet.Balance.TwoDigitDecimal());


                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModel = new SentSMS
                {
                    Mobile = Receiver.MobileNumber,
                    WhatsAppNumber = Receiver.WhatsAppNumber,
                    Message = messagewp,
                    UserID = Receiver.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModel, objModel.Type, message);
            }
        }



        public void SendSubscriptionPaymentDueMessage(Response.ServiceAndUserwiseSubscription.ViewModel model, Response.ServiceAndUserwiseSubscription.UserSubscriptionChargeViewModel submodel)
        {
            ViewModel setting = new ViewModel();

            setting = GetIsSMSOrMailOrWhatsUp(Enum.GetName(typeof(MessageSettingType), MessageSettingType.ServiceSubscriptionDuePayment));


            if (setting == null)
                return;


            UserMaster user = new UserMaster();
            try
            {
                user = new UserDAL().SelectByIDFromCache(model.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (user == null) return;

            string type = Enum.GetName(typeof(SMSType), SMSType.SubscriptionPayment);

            string message = setting.SmsMessage;
            if (setting.IsSMS)
            {
                message = setting.SmsMessage;
                message = message.Replace("[ServiceName]", submodel.ServiceName);
                message = message.Replace("[AutoDebit]", model.IsAutoDebitStr);
                message = message.Replace("[Amount]", submodel.Charge.TwoDigitDecimal());
                message = message.Replace("[Days]", submodel.DueDays.ToString());
                message = message.Replace("[LastPaymentDate]", submodel.PaymentEndDate);

                SentSMS objModel = new SentSMS
                {
                    Mobile = user.MobileNumber,
                    WhatsAppNumber = user.WhatsAppNumber,
                    Message = message,
                    UserID = user.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendSMS(objModel, objModel.Type, message);
            }

            if (setting.IsMail)
            {
                string mail = setting.EmailMessage;
                mail = mail.Replace("[ServiceName]", submodel.ServiceName);
                mail = mail.Replace("[AutoDebit]", model.IsAutoDebitStr);
                mail = mail.Replace("[Amount]", submodel.Charge.TwoDigitDecimal());
                mail = mail.Replace("[Days]", submodel.DueDays.ToString());
                mail = mail.Replace("[LastPaymentDate]", submodel.PaymentEndDate);


                MessageProcessing.SendEmail(user.EmailID, mail, type + user.FirmName);
            }

            if (setting.IsWhatsUp)
            {
                string messagewp = setting.WhatsUpMessage;
                messagewp = messagewp.Replace("[ServiceName]", submodel.ServiceName);
                messagewp = messagewp.Replace("[AutoDebit]", model.IsAutoDebitStr);
                messagewp = messagewp.Replace("[Amount]", submodel.Charge.TwoDigitDecimal());
                messagewp = messagewp.Replace("[Days]", submodel.DueDays.ToString());
                messagewp = messagewp.Replace("[LastPaymentDate]", submodel.PaymentEndDate);


                if (messagewp.Length <= 5)
                {
                    messagewp = message;
                }

                SentSMS objModel = new SentSMS
                {
                    Mobile = user.MobileNumber,
                    WhatsAppNumber = user.WhatsAppNumber,
                    Message = messagewp,
                    UserID = user.UserID,
                    Type = type,
                    TemplateID = setting?.TemplateID ?? "",
                };
                MessageProcessing.SendWhatsAppSMS(objModel, objModel.Type, messagewp);
            }
        }



    }
}