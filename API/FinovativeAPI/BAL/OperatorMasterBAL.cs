using Processing.Processing;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Operator;
using Project.Requests.PublicAPI;
using Project.Response.Operator;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class OperatorMasterBAL
    {
        public string Insert(OperatorInsertRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            if (mainRequest.Document.Size.Length > 0 && mainRequest.Document.Type.Length > 0)
                failureMessage = mainRequest.IsValidModalStateForDocument();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            string path = "";
            try
            {
                path = new MediaProcessing().SaveImage(mainRequest.Document, MediaType.OperatorImage, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            OperatorMaster ObjModel = new OperatorMaster
            {
                ServiceID = mainRequest.ServiceID,
                OperatorName = mainRequest.OperatorName,
                Opcode = mainRequest.Opcode,
                MinimumRechargeAmount = mainRequest.MinimumRechargeAmount,
                MaximumRechargeAmount = mainRequest.MaximumRechargeAmount,
                BusinessModel = mainRequest.BusinessModel,
                HSNCode = mainRequest.HSNCode,
                SequenceLogic = mainRequest.SequenceLogic,
                IsBBPS = mainRequest.IsBBPS,
                RepeatRechargeInMinute = 120,
                IsTimelyBlocked = false,
                BlockedTill = DateTime.Now.AddMinutes(-30),
                OperatorImagePath = (!string.IsNullOrEmpty(path)) ? path : "app-docs/OperatorImages/operator.svg?auto=compress&cs=tinysrgb&h=20",

                IsRandom = false,
                IsActive = true,
                IsDelete = false,
                IsBillFetch = mainRequest.IsBillFetch,
                IsPartialBill = mainRequest.IsPartialBill,
                MinLength = mainRequest.MinLength,
                MaxLength = mainRequest.MaxLength,
                TollFreeNumber = mainRequest.TollFreeNumber,
                AccountName = mainRequest.AccountName,
                Remark = mainRequest.Remark,
                AccountStartWith = mainRequest.AccountStartWith,
                AllowedFor = mainRequest.AllowedFor,
                UISequence = mainRequest.UISequence,
                BlockedAmount = mainRequest.BlockedAmount,
            };
            int rowsAffected;
            try
            {
                rowsAffected = new OperatorMasterDAL().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0) return OperatorMessage.InsertOperatorFailure;

            return StatusType.Ok;
        }

        internal Response.PublicAPI.GetOperatorByMobileNumberResponse GetOperatorByMobileNumber(GetOperatorByMobileNumberRequest selectRequest, out string failureMessage)
        {
            failureMessage = "";
            Response.PublicAPI.GetOperatorByMobileNumberResponse mainResponse = new Response.PublicAPI.GetOperatorByMobileNumberResponse();
            string URL = new CircleAPIDal().SelectActiveURL();

            if (URL.Length <= 5)
            {
                failureMessage = "API not available";
                return mainResponse;
            }

            URL = URL.Replace("[Number]", selectRequest.MobileNumber);
            CircleCore.Requests.GetCircleInfoRequest mainRequest = new CircleCore.Requests.GetCircleInfoRequest
            {
                Mobile = selectRequest.MobileNumber,
                Url = URL
            };

            CircleCore.Response.CoreResponse response = null;
            try
            {
                response = CircleCore.CircleProcessing.GetCircleInfo(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (response == null)
            {
                failureMessage = "Circle not available";
                return mainResponse;
            }

            ViewModel model = new ViewModel();
            List<ViewModel> operators = new List<ViewModel>();
            try
            {
                operators = new OperatorMasterDAL().SelectActiveOperatorOnly();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            model = operators.Where(o => o.OperatorName.ToLower().Contains(response.Operator.ToLower())).FirstOrDefault();

            mainResponse.OperatorName = model.OperatorName;
            mainResponse.OperatorID = model.OperatorID;
            mainResponse.CircleName = response.Circle;
            mainResponse.ServiceID = model.ServiceID;
            return mainResponse;
        }

        internal List<ViewModelForDropDown> SelectBBPSOperator(OperatorSelectByServiceIDRequest operatorSelectByService, out string failureMessage)
        {
            List<ViewModelForDropDown> operators = new List<ViewModelForDropDown>();
            failureMessage = OperatorMessage.OperatorsNotFound;

            try
            {
                operators = new OperatorMasterDAL().SelectBBPSOperator(operatorSelectByService.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (operators == null || operators.Count <= 0)
                return operators;

            failureMessage = StatusType.Ok;
            return operators;
        }

        internal string Delete(OperatorDeleteRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            OperatorMaster ObjModel = null;
            try
            {
                ObjModel = new OperatorMasterDAL().SelectByID(deleteRequest.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ObjModel == null)
                return OperatorMessage.OperatorsNotFound;

            ObjModel.IsDelete = true;
            ObjModel.IsActive = false;
            ObjModel.UpdatedDOC = DateTime.Now;

            int rowsAffected = OperatorUpdateCore(ObjModel);
            if (rowsAffected <= 0)
            {
                return OperatorMessage.OperatorDeleteFail;
            }

            return StatusType.Ok;
        }

        private int OperatorUpdateCore(OperatorMaster ObjModel)
        {
            if (ObjModel == null)
            {
                return 0;
            }

            try
            {
                int rowsAffected = new OperatorMasterDAL().Update(ObjModel);

                Task.Run(() => new FCMSelection().SendOperatorRefresh());

                return rowsAffected;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return 0;
        }

        internal string ActiveDeactive(OperatorDeleteRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            OperatorMaster ObjModel = null;
            try
            {
                ObjModel = new OperatorMasterDAL().SelectByID(deleteRequest.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return OperatorMessage.OperatorsNotFound;

            ObjModel.IsActive = deleteRequest.IsActive;
            ObjModel.UpdatedDOC = DateTime.Now;

            int rowsAffected = OperatorUpdateCore(ObjModel);
            if (rowsAffected <= 0)
            {
                return OperatorMessage.OperatorActiveDeactiveFail;
            }

            return StatusType.Ok;
        }

        internal string Update(OperatorUpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (updateRequest.Document.Size.Length > 0 && updateRequest.Document.Type.Length > 0)
                failureMessage = updateRequest.IsValidModalStateForDocument();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            string path = "";

            try
            {
                path = new MediaProcessing().SaveImage(updateRequest.Document, MediaType.OperatorImage, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            OperatorMaster ObjModel;
            try
            {
                ObjModel = new OperatorMasterDAL().SelectByID(updateRequest.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return OperatorMessage.OperatorsNotFound;

            ObjModel.OperatorName = updateRequest.OperatorName;
            ObjModel.Opcode = updateRequest.Opcode;
            ObjModel.MinimumRechargeAmount = updateRequest.MinimumRechargeAmount;
            ObjModel.MaximumRechargeAmount = updateRequest.MaximumRechargeAmount;
            ObjModel.RepeatRechargeInMinute = updateRequest.RepeatRechargeInMinute;
            ObjModel.BusinessModel = updateRequest.BusinessModel;
            ObjModel.HSNCode = updateRequest.HSNCode ?? "";
            ObjModel.SequenceLogic = updateRequest.SequenceLogic ?? "";
            // ObjModel.Prefix = updateRequest.AccountStartWith;
            ObjModel.IsBBPS = updateRequest.IsBBPS;
            ObjModel.OperatorImagePath = (!string.IsNullOrEmpty(path)) ? path : ObjModel.OperatorImagePath;
            ObjModel.UpdatedDOC = DateTime.Now;
            ObjModel.IsBillFetch = updateRequest.IsBillFetch;
            ObjModel.IsPartialBill = updateRequest.IsPartialBill;
            ObjModel.MinLength = updateRequest.MinLength;
            ObjModel.MaxLength = updateRequest.MaxLength;
            ObjModel.TollFreeNumber = updateRequest.TollFreeNumber;
            ObjModel.AccountName = updateRequest.AccountName;
            ObjModel.Remark = updateRequest.Remark;
            ObjModel.AccountStartWith = updateRequest.AccountStartWith;
            ObjModel.UISequence = updateRequest.UISequence;
            ObjModel.AllowedFor = updateRequest.AllowedFor;
            ObjModel.BlockedAmount = updateRequest.BlockedAmount;
            ObjModel.IsViewRequired = updateRequest.IsViewRequired;


            int rowsAffected = OperatorUpdateCore(ObjModel);
            if (rowsAffected <= 0)
            {
                return OperatorMessage.OperatorUpdateFail;
            }
            int ServiceID = new OperatorMasterBAL().SelectServiceID(updateRequest.OperatorID);
            if (IsUpdateSlab(ServiceID))
            {
                Task.Run(() => new SlabDetailDAL().BulkUpdateMaxMinAmount(updateRequest.OperatorID, updateRequest.MinimumRechargeAmount, updateRequest.MaximumRechargeAmount));
            }

            return StatusType.Ok;
        }

        private bool IsUpdateSlab(int ServiceID)
        {
            Requests.Service.ServiceTypeRequest sser = new Requests.Service.ServiceTypeRequest();
            sser.ServiceType = ServiceType.Recharge;

            List<Response.Service.ViewModel> ReturnList = new ServiceMasterDal().SelectByServicesType(sser);

            foreach (Response.Service.ViewModel item in ReturnList)
            {
                if (item.ServiceID == ServiceID)
                {
                    return true;
                }
            }

            return false;
        }

        internal int SelectServiceID(int operatorID)
        {
            return new OperatorMasterDAL().SelectServiceID(operatorID);
        }

        internal bool IsOperatorOn(int operatorID)
        {
            ViewModel operators = new OperatorMasterDAL().SelectDetailsByOperatorID(operatorID);
            return operators.IsActive;
        }

        internal bool IsValidAmountForMaxMinRechargeAmount(int operatorID, decimal amount)
        {
            ViewModel ObjModel = new OperatorMasterDAL().SelectDetailsByOperatorID(operatorID);

            return amount >= ObjModel.MinimumRechargeAmount && amount <= ObjModel.MaximumRechargeAmount;
        }

        internal List<ViewModelForDropDown> SelectByServiceID(OperatorSelectByServiceIDRequest requestModel, int UserID, out string failureMessage)
        {
            List<ViewModelForDropDown> operators = new List<ViewModelForDropDown>();
            failureMessage = requestModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return operators;
            try
            {
                operators = new OperatorMasterDAL().SelectByServiceID(requestModel.ServiceID, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return operators;
        }
        internal List<ViewModelForDropDown> SelectAEPSOperator(OperatorSelectByServiceIDRequest requestModel,out string failureMessage)
        {
            List<ViewModelForDropDown> operators = new List<ViewModelForDropDown>();
            failureMessage = requestModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return operators;
            try
            {
                operators = new OperatorMasterDAL().SelectAEPSOperator(requestModel.ServiceID);
                
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return operators;
        }

        internal List<ViewModelForDropDown> SelectOperatorByServiceID(OperatorSelectByServiceIDRequest requestModel, out string failureMessage)
        {
            List<ViewModelForDropDown> operators = new List<ViewModelForDropDown>();
            failureMessage = requestModel.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return operators;
            try
            {
                operators = new OperatorMasterDAL().SelectOperatorByServiceID(requestModel.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return operators;
        }

        internal List<ViewModel> SelectOpertorListByServiceID(int ServiceID)
        {
            List<ViewModel> operators = new List<ViewModel>();

            try
            {
                operators = new OperatorMasterDAL().SelectOperatorListByServiceID(ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return operators;
        }

        internal List<ViewModel> SelectOperator()
        {
            List<ViewModel> operators = new List<ViewModel>();

            try
            {
                operators = new OperatorMasterDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return operators;
        }

        internal List<ViewModel> SelectOperatorByServiceID(SelectOperatorRequest mainRequest)
        {
            List<ViewModel> operators = new List<ViewModel>();

            try
            {
                operators = new OperatorMasterDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (operators.Count <= 0)
                return operators;

            if (mainRequest.ServiceID <= 0) return operators;

            operators = operators.Where(o => o.ServiceID == mainRequest.ServiceID).ToList();

            return operators;
        }

        internal OperatorWrapper SelectOperatorForExcel(OperatorRequest mainRequest)
        {
            OperatorWrapper wrapper = new OperatorWrapper();
            List<ViewModel> operators = new List<ViewModel>();

            try
            {
                operators = new OperatorMasterDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (operators.Count <= 0)
                return wrapper;

            if (mainRequest.ServiceID <= 0) return wrapper;

            operators = operators.Where(o => o.ServiceID == mainRequest.ServiceID).ToList();

            wrapper.list = operators;
            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;

            return wrapper;
        }

        internal List<ViewModel> SelectDeactiveOperators()
        {
            List<ViewModel> operators = new List<ViewModel>();
            try
            {
                operators = new OperatorMasterDAL().Select().Where(P => (P.ServiceID == (int)ServiceCode.Mobile ||
                P.ServiceID == (int)ServiceCode.Dth) && (P.IsActive == false)).ToList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (operators.Count <= 0)
                return operators;

            return operators;
        }

        internal ViewModel SelectByOperatorID(OperatorDeleteRequest selectRequest, out string failureMessage)
        {
            ViewModel operators = new ViewModel();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return operators;
            try
            {
                operators = new OperatorMasterDAL().SelectDetailsByOperatorID(selectRequest.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return operators;
        }

        internal bool IsBBPSOperator(int OperatorID)
        {
            ViewModel ObjModel = null;

            try
            {
                ObjModel = new OperatorMasterDAL().SelectDetailsByOperatorID(OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
                return false;
            if (!ObjModel.IsBBPS)
                return false;
            return true;
        }
        internal bool IsViewRequired(int OperatorID)
        {
            ViewModel ObjModel = null;
            try
            {
                ObjModel = new OperatorMasterDAL().SelectDetailsByOperatorID(OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
                return false;
            if (!ObjModel.IsViewRequired)
                return false;
            return true;
        }

        internal bool IsValid(string ConsumerNumber, int OperatorID, decimal Amount, out string failureMessage)
        {
            failureMessage = TransactionMessage.InvalidConsumerNumber;
            bool isFound = false;
            try
            {
                ViewModel ObjModel = new OperatorMasterDAL().SelectDetailsByOperatorID(OperatorID);

                if (ObjModel == null) return false;

                failureMessage = !string.IsNullOrEmpty(ObjModel.Remark) ? ObjModel.Remark : failureMessage;

                failureMessage = "Consumer number should greater than " + ObjModel.MinLength.ToString();

                if (ConsumerNumber.Length < ObjModel.MinLength) return false;

                failureMessage = "Consumer number should less than " + ObjModel.MaxLength.ToString();

                if (ConsumerNumber.Length > ObjModel.MaxLength) return false;

                failureMessage = "Amount should greater than " + ObjModel.MinimumRechargeAmount.TwoDigitDecimal();

                if (Amount < ObjModel.MinimumRechargeAmount) return false;

                failureMessage = "Amount should less than " + ObjModel.MaximumRechargeAmount.TwoDigitDecimal();

                if (Amount > ObjModel.MaximumRechargeAmount) return false;

                if (ObjModel.AccountStartWith == null || ObjModel.AccountStartWith.Length <= 0)
                    return true;

                failureMessage = StatusType.Ok;
                List<string> startsWithList = ObjModel.AccountStartWith.Split(',').ToList();

                if (startsWithList.Count <= 0) return true;

                failureMessage = "Account should start with " + ObjModel.AccountStartWith;

                foreach (string startswith in startsWithList)
                {
                    if (ConsumerNumber.StartsWith(startswith.Trim()))
                    {
                        isFound = true;
                        failureMessage = StatusType.Ok;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return isFound;
        }

        internal List<ViewModelDropDownResponse> SelectForDropDown(SelectOperatorForDropDownRequest mainRequest, out string failureMessage)
        {
            List<ViewModelDropDownResponse> list = null;
            failureMessage = "Operator not found.";

            try
            {
                list = new OperatorMasterDAL().SelectOperatorForDropDown();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }

            if (list == null)
                return list;

            if (list.Count <= 0)
                return list;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.OperatorName))
                    list = list.AsQueryable().WhereDynamic(mainRequest.OperatorName).ToList();
            }

            failureMessage = StatusType.Ok;
            return list;
        }

        internal List<ViewModelDropDownResponse> SelectOperatorPrepaidAndDTH()
        {
            List<ViewModelDropDownResponse> list = null;
            try
            {
                list = new OperatorMasterDAL().SelectOperatorPrepaidAndDTH();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return list;
            }

            return list;
        }

        internal List<TollFreeDetailsViewModel> SelectOperatorWithTollFreeDetails(OperatorSelectByServiceIDRequest item)
        {
            List<TollFreeDetailsViewModel> list = null;
            try
            {
                list = new OperatorMasterDAL().SelectOperatorWithTollFreeDetails(item);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return list;
            }

            return list;
        }

        internal bool CheckFileExists(string path)
        {
            OperatorMaster response = null;
            try
            {
                response = new OperatorMasterDAL().SelectByImageURL(path);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return true;
            }

            if (response == null) return false;

            return true;
        }

        internal OperatorMaster SelectByOperatorID(int OperatorID)
        {
            OperatorMaster operators = null;

            try
            {
                operators = new OperatorMasterDAL().SelectByID(OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return operators;
        }
    }
}