using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.EmailSetting;
using Project.Requests.EmailSettingMaster;
using Project.Response.EmailSettingMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class EmailSettingBAL
    {
        public string Insert(InsertRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            EmailSettingMaster objModel = new EmailSettingDal().SelectByEmail(mainRequest.Email);

            failureMessage = Message.EmailSettingMessage.AlreadyExist;

            if (objModel != null) return "";

            failureMessage = Message.EmailSettingMessage.InsertFail;

            objModel = new EmailSettingMaster
            {
                Email = mainRequest.Email,
                Password = mainRequest.Password,
                HostName = mainRequest.HostName,
                Port = mainRequest.Port,
                IsActive = true,
                IsSSL = false
            };

            int Result;

            try
            {
                Result = new EmailSettingDal().Insert(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return Message.EmailSettingMessage.InsertSuccess;
        }

        public string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            failureMessage = Message.EmailSettingMessage.AlreadyExist;

            EmailSettingMaster objModel = new EmailSettingDal().SelectByID(mainRequest.EmailSettingMasterID);

            if (objModel == null)
                return "";

            failureMessage = Message.EmailSettingMessage.UpdateFail;

            objModel.Email = mainRequest.Email;
            objModel.Password = mainRequest.Password;
            objModel.HostName = mainRequest.HostName;
            objModel.Port = mainRequest.Port;
            objModel.IsActive = mainRequest.IsActive;
            objModel.IsSSL = mainRequest.IsSSL;

            int Result;
            try
            {
                Result = new EmailSettingDal().Update(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;

            return Message.EmailSettingMessage.UpdateSuccess;
        }

        public List<ViewModel> Select(out string failureMessage)
        {
            failureMessage = Message.EmailSettingMessage.NotFound;

            List<ViewModel> emails = new List<ViewModel>();
            List<EmailSettingMaster> list;

            try
            {
                list = new EmailSettingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return emails;
            }

            if (list == null || list.Count <= 0) return emails;

            foreach (EmailSettingMaster email in list)
            {
                try
                {
                    emails.Add(new ViewModel
                    {
                        EmailSettingMasterID = email.EmailSettingMasterID,
                        Email = email.Email,
                        Password = email.Password,
                        HostName = email.HostName,
                        Port = email.Port,
                        IsSSL = email.IsSSL,
                        IsActive = email.IsActive
                    });
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return emails;
                }
            }

            failureMessage = StatusType.Ok;
            return emails;
        }

        public ViewModel SelectByID(int EmailSettingMasterID, out string failureMessage)
        {
            failureMessage = Message.EmailSettingMessage.NotFound;

            ViewModel emails = new ViewModel();
            EmailSettingMaster dbObj = null;

            try
            {
                dbObj = new EmailSettingDal().SelectByID(EmailSettingMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return emails;
            }

            if (dbObj == null)
                return emails;

            emails.EmailSettingMasterID = dbObj.EmailSettingMasterID;
            emails.Email = dbObj.Email;
            emails.Password = dbObj.Password;
            emails.HostName = dbObj.HostName;
            emails.Port = dbObj.Port;
            emails.IsSSL = dbObj.IsSSL;
            emails.IsActive = dbObj.IsActive;

            failureMessage = StatusType.Ok;
            return emails;
        }

        internal string ActiveDeactiveSSL(ActiveDeactiveSSLRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            failureMessage = Message.EmailSettingMessage.AlreadyExist;

            EmailSettingMaster objModel = new EmailSettingDal().SelectByID(mainRequest.EmailSettingMasterID);

            if (objModel == null)
                return "";

            failureMessage = Message.EmailSettingMessage.UpdateFail;
            bool dbVal = objModel.IsSSL;

            objModel.IsSSL = !dbVal;

            int Result;
            try
            {
                Result = new EmailSettingDal().Update(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;

            return Message.EmailSettingMessage.UpdateSuccess;
        }

        internal string ActiveDeactive(ActiveDeactiveRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            failureMessage = Message.EmailSettingMessage.AlreadyExist;

            EmailSettingMaster objModel = new EmailSettingDal().SelectByID(mainRequest.EmailSettingMasterID);

            if (objModel == null)
                return "";

            failureMessage = Message.EmailSettingMessage.UpdateFail;
            bool dbVal = objModel.IsActive;
            objModel.IsActive = !dbVal;

            int Result;
            try
            {
                Result = new EmailSettingDal().Update(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;

            return Message.EmailSettingMessage.UpdateSuccess;
        }
    }
}