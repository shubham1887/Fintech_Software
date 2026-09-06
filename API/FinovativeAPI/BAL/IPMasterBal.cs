using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.IPMaster;
using Project.Response.IPMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class IPMasterBal
    {
        internal string Insert(InsertRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;
            if (!new Validation().IsValidIP(mainRequest.IP))
            {
                failureMessage = IPMasterMessage.InvalidIP;
                return response;
            }
            bool IsValidOTP = new OTPMasterBal().IsValid(keyDataModel.UserID, mainRequest.OTP, Enum.GetName(typeof(StaticModel.OTPType), StaticModel.OTPType.IPMaster));

            if (!IsValidOTP)
            {
                failureMessage = Messages.InvalidOTP;
                return response;
            }

            IPMaster objModel = new IPMaster
            {
                UserID = keyDataModel.UserID,
                IP = mainRequest.IP,
                Status = StatusType.Process,
            };

            IPMaster iPMaster = null;

            try
            {
                iPMaster = new IPMasterDal().SelectByUserID(keyDataModel.UserID, mainRequest.IpMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            int Result;

            if (iPMaster == null)
            {
                response = Message.IPMasterMessage.InsertFail;
                try
                {
                    Result = new IPMasterDal().Insert(objModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return response; ;
                }
                if (Result > 0)
                    response = Message.IPMasterMessage.InsertSuccess;
                else
                    failureMessage = response;
            }
            else
            {
                response = Message.IPMasterMessage.UpdateFail;
                iPMaster.IP = mainRequest.IP;
                iPMaster.Status = StatusType.Process;
                try
                {
                    Result = new IPMasterDal().Update(iPMaster);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return response;
                }
                if (Result > 0)
                    response = Message.IPMasterMessage.UpdateSuccess;
                else
                    failureMessage = response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal bool IsValidIPAddress(string iPAddress, int userID)
        {
            IPMaster usersIp;
            try
            {
                List<IPMaster> iPs = new IPMasterDal().SelectByUserID(userID);

                if (iPs == null || iPs.Count <= 0)
                    return true;

                usersIp = new IPMasterDal().SelectByIP(userID, iPAddress);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }
            if (usersIp == null)
                return false;

            return true;
        }

        internal List<ViewModel> Select(int userID, out string failureMessage)

        {
            failureMessage = "";
            List<ViewModel> report = null;
            List<IPMaster> list = null;

            try
            {
                list = new IPMasterDal().SelectByUserID(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.DataNotFound;
                return report;
            }

            if (list == null)
            {
                failureMessage = Messages.DataNotFound;
                return report;
            }
            report = new List<ViewModel>();
            foreach (IPMaster item in list)
            {
                report.Add(new ViewModel
                {
                    Date = item.DOC,
                    UpdatedDate = item.UpdatedDOC,
                    IPAddress = item.IP,
                    IPMasterID = item.IpMasterID,
                    UserID = item.UserID,
                    Status = item.Status,
                });
            }

            failureMessage = StatusType.Ok;
            return report;
        }

        internal string Delete(DeleteRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;
            int Result;
            failureMessage = Message.IPMasterMessage.DeleleFail;
            try
            {
                Result = new IPMasterDal().Delete(mainRequest.IpMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response; ;
            }
            if (Result > 0)
                response = Message.IPMasterMessage.DeleleSuccess;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<ViewModel> SelectProcessIp(int UserID, SelectRequest selectRequest, out string failureMessage)
        {
            failureMessage = "";
            List<ViewModel> report = null;
            if (UserID != Company.AdminUserID)
            {
                failureMessage = Message.UserMessage.InvalidUser;
                return report;
            }
            try
            {
                report = new IPMasterDal().SelectProcessIp(selectRequest.Status);

                if (report == null || report.Count <= 0)
                {
                    failureMessage = Messages.DataNotFound;
                    return report;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return report;
            }
            failureMessage = StatusType.Ok;
            return report;
        }

        internal string UpdateIpStatus(UpdateStatusRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            IPMaster ObjModel = null;
            try
            {
                ObjModel = new IPMasterDal().SelectByID(updateRequest.IPMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            ObjModel.Status = updateRequest.Status;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new IPMasterDal().Update(ObjModel);

                if (rowsAffected <= 0)
                {
                    return Message.IPMasterMessage.UpdateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }
    }
}