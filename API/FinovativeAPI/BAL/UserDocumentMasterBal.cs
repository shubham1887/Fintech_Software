using DocumentFormat.OpenXml.Spreadsheet;
using Processing.Processing;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.KYC;
using Project.Response.KYC;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class UserDocumentMasterBal
    {
        internal string Upload(UploadRequest mainRequest, KeyDataModel keyDataModel)
        {
            if (mainRequest.Document.Size.Length <= 0 && mainRequest.Document.Type.Length <= 0)
                return KYCMessage.InvalidDocument;

            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            failureMessage = Message.KYCMessage.InvalidDocumentNo;
            if (!IsValidDocumentNo(mainRequest.KycDocumentMasterID, mainRequest.DocumentNo))
                return failureMessage;

            if (mainRequest.SelectedUserID == (int)UserType.Admin)
                return UserMessage.UserNotFound;

            if (mainRequest.SelectedUserID != 0)
                keyDataModel.UserID = mainRequest.SelectedUserID;

            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return UserMessage.UserNotFound;
            }
            if (user == null)
            {
                return UserMessage.UserNotFound;
            }

            KycDocumentMaster doctype = null;
            try
            {
                doctype = new KycDocumentMasterDal().SelectByID(mainRequest.KycDocumentMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return KYCMessage.InvalidDocumentType;
            }
            if (doctype == null)
            {
                return KYCMessage.InvalidDocumentType;
            }

            mainRequest.DocumentType = doctype.DocumentType;
            UserDocumentMaster ObjModel = new UserDocumentMasterDal().SelectByUserAndDocType(keyDataModel.UserID, mainRequest.DocumentType);

            if (ObjModel != null)
            {
                if (ObjModel.Status != null && ObjModel.Status.EqualsIgnoreCase(StatusType.Approve))
                    return KYCMessage.AlreadyApprove;
            }

            string path = "";

            try
            {
                path = new MediaProcessing().SaveImage(mainRequest.Document, mainRequest.DocumentType, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return KYCMessage.DocumentnotUpload;
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            if (string.IsNullOrEmpty(path))
            {
                return KYCMessage.DocumentnotUpload;
            }

            failureMessage = Message.KYCMessage.UploadFail;
            int Result = -1;

            if (ObjModel != null)
            {
                ObjModel.DocumentNo = mainRequest.DocumentNo;
                ObjModel.ImageURL = path;
                ObjModel.Status = StatusType.Process;
                Result = new UserDocumentMasterDal().Update(ObjModel);
            }
            else
            {
                UserDocumentMaster ObjModelModified = new UserDocumentMaster
                {
                    UserID = keyDataModel.UserID,
                    DocmentType = mainRequest.DocumentType,
                    DocumentNo = mainRequest.DocumentNo,
                    ImageURL = path,
                    Status = StatusType.Process,
                };
                try
                {
                    Result = new UserDocumentMasterDal().Insert(ObjModelModified);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Message.KYCMessage.UploadFail;
                }
            }

            if (Result <= 0)
            {
                return Message.KYCMessage.UploadFail;
            }
            try
            {
                Task.Run(() => new MessageSelection().SendKYCPendingMail(user));
                Task.Run(() => new FCMSelection().SendKYCRequest());
            }
            catch (Exception)
            {
            }

            return StatusType.Ok;
        }

        internal bool IsValidDocumentNo(int KycDocumentMasterID, string DocumentNo)
        {
            KycDocumentMaster ObjModel = null;
            try
            {
                ObjModel = new KycDocumentMasterDal().SelectByID(KycDocumentMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return false;
            }
            if (ObjModel.RegularRxpression == null || ObjModel.RegularRxpression.EqualsIgnoreCase("") || ObjModel.RegularRxpression.Length <= 3)
                return true;

            Regex regex = new Regex(ObjModel.RegularRxpression);
            Match match = regex.Match(DocumentNo);
            if (!match.Success) return false;

            return true;
        }

        internal string Approve(ApproveRequest mainRequest, KeyDataModel keyDataModel)
        {
            string failureMessage = UserMessage.UserNotFound;

            int UserTypeID = new UserDAL().SelectUserTypeID(keyDataModel.UserID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
                return TransactionMessage.InvalidUser;
            int UserDocumentID = -1;

            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(mainRequest.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (user == null)
                return failureMessage;

            UserDocumentMaster userdocument = null;
            try
            {
                userdocument = new UserDocumentMasterDal().SelectByKYCID(mainRequest.UserDocumentID);

                userdocument.Status = StatusType.Reject;

                if (mainRequest.IsApproved == true)
                    userdocument.Status = StatusType.Approve;
                userdocument.Remark = mainRequest.Remark;

                UserDocumentID = new UserDocumentMasterDal().Update(userdocument);
                if (UserDocumentID < 0)
                    return "Unable to update";
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (userdocument.Status.EqualsIgnoreCase(StatusType.Approve))
            {
                if (user.AadharNo.Length <= 8)
                {
                    if (userdocument.DocmentType.ToLower().Contains("AADHAAR".ToLower()))
                        user.AadharNo = userdocument.DocumentNo;
                }

                if (user.PANNo.Length <= 7)
                {
                    if (userdocument.DocmentType.ToLower().Contains("PAN".ToLower()))
                        user.PANNo = userdocument.DocumentNo;
                }

                if (user.GSTNo.Length <= 7)
                {
                    if (userdocument.DocmentType.ToLower().Contains("GST".ToLower()))
                        user.GSTNo = userdocument.DocumentNo;
                }

                try
                {
                    new UserDAL().Update(user);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            if (userdocument.Status.EqualsIgnoreCase(StatusType.Reject))
            {
                new MessageSelection().SendKYCApproveRejectMessage(mainRequest.IsApproved, user, mainRequest.Remark);

                failureMessage = StatusType.Ok;
                return Message.KYCMessage.KYCStatusUploadSuccess;
            }

            bool requiredAllKycApprove = false;
            try
            {
                List<KycDocumentMaster> documentlist = null;

                documentlist = new KycDocumentMasterDal().SelectType();
                foreach (KycDocumentMaster item in documentlist)
                {
                    requiredAllKycApprove = new UserDocumentMasterDal().SelectByUserID(mainRequest.UserID, item.DocumentType);
                    if (requiredAllKycApprove == false)
                        break;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

           
            if (requiredAllKycApprove == true)
            {
                user.IsKYCApproved = mainRequest.IsApproved;
                try
                {
                    new UserDAL().Update(user);

                    new KYCApprovedDetailsBal().Insert(keyDataModel.UserID, mainRequest.UserID, mainRequest.Remark);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                new MessageSelection().SendKYCApproveRejectMessage(mainRequest.IsApproved, user, mainRequest.Remark);
            }
            try
            {
                new FCMSelection().SendKYCNotification(mainRequest.IsApproved, user, UserMessage.KYCAcceptorReject);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return StatusType.Ok;
        }

        internal KYCWrapper Select(SelectRequest mainRequest)
         {
            KYCWrapper wrapper = new KYCWrapper();
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new UserDocumentMasterDal().Select(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            list = list.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            int count = list.Count();
            if (!mainRequest.IsExcelDownload)
            {
                
                list = list.AsQueryable().SkipAndTakeCount(mainRequest.SkipCount, mainRequest.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = list;
            wrapper.Count = count;
            return wrapper;
        }

        internal List<ViewModel> selectDocumentsByUserID(int UserID)
        {
            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new UserDocumentMasterDal().SelectDocumentsByUserID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return list;
        }

        internal List<ViewModel> SelectPending()
        {
            List<ViewModel> list = new List<ViewModel>();
            try
            {
                // list = new UserDocumentMasterDal().SelectPending(mainRequest);
                list = new UserDocumentMasterDal().SelectKYCPendingUsers();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return list;
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel response = null;

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                response = new UserDocumentMasterDal().SelectByID(mainRequest.UserDocumentID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = KYCMessage.NotFound;
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<KYCViewModel> SelectActiveDocUserwise(int UserID, out string failureMessage)
        {
            List<KYCViewModel> viewModel = new List<KYCViewModel>();
            try
            {
                viewModel = new UserDocumentMasterDal().SelectActiveDocUserwise(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (viewModel.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return viewModel;
            }
            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal bool CheckFileExists(string path)
        {
            UserDocumentMaster response = null;
            try
            {
                response = new UserDocumentMasterDal().SelectByImageURL(path);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return true;
            }

            if (response == null) return false;

            return true;
        }

        internal string ReKyc(int UserID)
        {
            UserDocumentMaster userdocument = null;
            List<ViewModel> list = new UserDocumentMasterDal().SelectDocumentsByUserID(UserID);
            try
            {
                foreach (ViewModel item in list)
                {
                    userdocument = new UserDocumentMasterDal().SelectByKYCID(item.UserDocumentID);
                    userdocument.Status = StatusType.Process;
                    new UserDocumentMasterDal().Update(userdocument);
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