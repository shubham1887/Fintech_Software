using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Service;
using Project.Response.Service;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class ServiceMasterBal
    {
        internal List<Response.Service.ViewModel> Select(int UserID)
        {
            List<Response.Service.ViewModel> ReturnList = null;
            try
            {
                ReturnList = new ServiceMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            string UserwiseServiceOff = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.UserwiseServiceOff), "No");
            if (!UserwiseServiceOff.EqualsIgnoreCase("Yes"))
                return ReturnList;

            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);
            if (user.UserTypeID != (int)UserType.Retailer)
                return ReturnList;
            List<Requests.User.UserServiceMappingDetailList> mainList = new UserServiceMappingDal().SelectByUserID(UserID);
            foreach (Response.Service.ViewModel item in ReturnList)
            {
                if (!IsValidUserServiceMapping(item.ServiceID, mainList))
                {
                    item.IsDisplay = false;
                }
            }

            return ReturnList;
        }
        private bool IsValidUserServiceMapping(int ServiceID, List<Requests.User.UserServiceMappingDetailList> mainList)
        {
            Requests.User.UserServiceMappingDetailList reponse = null;
            reponse = (from User in mainList
                       where User.ServiceID == ServiceID
                       select User).FirstOrDefault();
            if (reponse == null)
                return false;
            return true;
        }

        internal bool IsServiceOn(int serviceID)
        {
            ServiceMaster ObjModel = new ServiceMasterDal().SelectByID(serviceID);
            if (ObjModel == null)
                return false;
            if (!ObjModel.IsActive)
                return false;
            return true;
        }

        internal string Activate(ActivateRequest request)
        {
            string failureMessage = request.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }

            failureMessage = Message.ServiceMessage.NotFound;

            ServiceMaster ObjModel = null;
            try
            {
                ObjModel = new ServiceMasterDal().SelectByID(request.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            ObjModel.IsActive = request.IsActive;
            int Result = -1;
            try
            {
                Result = new ServiceMasterDal().Update(ObjModel);

                Task.Run(() => new FCMSelection().SendOperatorRefresh());
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
            {
                return Message.ServiceMessage.UpdateFail;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        public string Insert(Requests.Service.InsertRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ServiceMaster ObjModel = new ServiceMaster
            {
                Name = mainRequest.ServiceName,
                Charge = mainRequest.Charge,
                IsActive = true,
                IsDelete = false,
            };
            try
            {
                ServiceMaster isServiceExits = new ServiceMasterDal().SelectByServiceName(mainRequest.ServiceName);
                if (isServiceExits != null)
                {
                    return ServiceMessage.NameAlreadyExits;
                }

                int rowsAfftected = new ServiceMasterDal().Insert(ObjModel);

                if (rowsAfftected <= 0)
                {
                    return ServiceMessage.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            return StatusType.Ok;
        }

        internal string Update(Requests.Service.UpdateRequest request)
        {
            string failureMessage = request.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = Message.ServiceMessage.NotFound;

            ServiceMaster ObjModel = null;
            try
            {
                ObjModel = new ServiceMasterDal().SelectByID(request.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.Name = request.ServiceName;
            ObjModel.Charge = request.Charge;
            ObjModel.MinBalance = request.MinBalance;
            ObjModel.ServiceCategory = request.ServiceCategory;
            int Result = -1;
            try
            {
                Result = new ServiceMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return Message.ServiceMessage.UpdateFail;

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal Response.Service.ViewModel SelectByServiceID(SelectRequest request)
        {
            Response.Service.ViewModel modelObj = new Response.Service.ViewModel();
            ServiceMaster model = null;
            try
            {
                model = new ServiceMasterDal().SelectByID(request.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return modelObj;
            }
            if (model == null)
            {
                return modelObj;
            }
            modelObj.ServiceID = model.ServiceID;
            modelObj.ServiceName = model.Name;
            modelObj.Charge = model.Charge;
            modelObj.MinBalance = model.MinBalance;
            modelObj.ServiceCategory = model.ServiceCategory;

            return modelObj;
        }



        internal ServiceAllActiveDeactiveOtpResponse GenerateAllServiceActiveDeactiveOTP(int userID)
        {
            ServiceAllActiveDeactiveOtpResponse response = null;

            UserMaster sender = new UserDAL().SelectByIDFromCache(userID);
            Task.Run(() => new MessageSelection().SendOTPForSerivesActiveDeActive(sender));


            response = new ServiceAllActiveDeactiveOtpResponse()
            {
                IsOTPSent = true,
                Message = StatusType.Ok,
            };
            return response;
        }



        internal async Task<string> AllServiceActiveDeactiveOTPRequest(AllServiceActiveDeactiveOTPRequest mainRequest, int UserID)
        {
            string failureMessage = Message.UserMessage.UserNotFound;

            UserMaster userMaster = null;
            try
            {
                userMaster = new UserDAL().SelectByID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (userMaster == null) return failureMessage;

            bool IsValidOTP = new OTPMasterBal().IsValid(userMaster.UserID, mainRequest.OTP, Enum.GetName(typeof(OTPType), OTPType.ServiceActivateDeactivate));
            if (!IsValidOTP)
                return UserMessage.InvalidOTP;




            List<ServiceMaster> objModel = new ServiceMasterDal().SelectAll();
            if (objModel == null || objModel.Count <= 0)
            {
                return failureMessage;
            }


            foreach (ServiceMaster viewmodel in objModel)
            {
                viewmodel.IsActive = mainRequest.IsActive;
            }

            Task.Run(() =>
            {
                new ServiceMasterDal().UpdateRange(objModel);
            });


            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal List<Response.Service.ViewModel> SelectByRechargeServices()
        {
            List<Response.Service.ViewModel> ReturnList = null;
            try
            {
                ReturnList = new ServiceMasterDal().SelectByRechargeServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }

        internal List<Response.Service.ViewModel> SelectByRechargeDTH()
        {
            List<Response.Service.ViewModel> ReturnList = null;
            try
            {
                ReturnList = new ServiceMasterDal().SelectByRechargeDTH();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }

        internal List<Response.Service.ViewModel> SelectChargeServices()
        {
            List<Response.Service.ViewModel> ReturnList = null;
            try
            {
                ReturnList = new ServiceMasterDal().SelectChargeServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }

        internal List<ViewModel> SelectByServicesType(ServiceTypeRequest mainRequest)
        {
            List<Response.Service.ViewModel> ReturnList = null;
            try
            {
                ReturnList = new ServiceMasterDal().SelectByServicesType(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }

        internal List<ViewModel> SelectByOfflineServices()
        {
            List<Response.Service.ViewModel> ReturnList = null;
            try
            {
                ReturnList = new ServiceMasterDal().SelectByOfflineServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }

        internal List<Response.Service.ViewModel> SelectByPrimeServices()
        {
            List<Response.Service.ViewModel> ReturnList = null;
            try
            {
                ReturnList = new ServiceMasterDal().SelectByPrimeServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }

        internal List<ViewModel> SelectPrePaidDTHFRCService()
        {
            List<Response.Service.ViewModel> ReturnList = null;
            try
            {
                ReturnList = new ServiceMasterDal().SelectPrePaidDTHFRCService();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }

        internal List<Response.Service.ViewModel> SelectForCallBackUrl()
        {
            List<Response.Service.ViewModel> ReturnList = null;
            try
            {
                ReturnList = new ServiceMasterDal().SelectForCallBackUrl();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }

        internal string Display(DisplayRequest request)
        {
            string failureMessage = request.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }

            failureMessage = Message.ServiceMessage.NotFound;

            ServiceMaster ObjModel = null;
            try
            {
                ObjModel = new ServiceMasterDal().SelectByID(request.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            ObjModel.IsDisplay = request.IsDisplay;
            int Result = -1;
            try
            {
                Result = new ServiceMasterDal().Update(ObjModel);

                Task.Run(() => new FCMSelection().SendOperatorRefresh());
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
            {
                return Message.ServiceMessage.UpdateFail;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string DefaultIsOnOff(DefaultRequest request)
        {
            string failureMessage = request.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }

            failureMessage = Message.ServiceMessage.NotFound;

            ServiceMaster ObjModel = null;
            try
            {
                ObjModel = new ServiceMasterDal().SelectByID(request.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            ObjModel.IsDefault = request.IsDefault;
            int Result = -1;
            try
            {
                Result = new ServiceMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
            {
                return Message.ServiceMessage.UpdateFail;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string OutsideIsOnOff(OutsideDefaultRequest request)
        {
            string failureMessage = request.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }

            failureMessage = Message.ServiceMessage.NotFound;

            ServiceMaster ObjModel = null;
            try
            {
                ObjModel = new ServiceMasterDal().SelectByID(request.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            ObjModel.IsOutside = request.IsOutside;
            int Result = -1;
            try
            {
                Result = new ServiceMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
            {
                return Message.ServiceMessage.UpdateFail;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }


        internal string SubscriptionAllowOnOFF(SubscriptionAllowRequest request)
        {
            string failureMessage = request.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }

            failureMessage = Message.ServiceMessage.NotFound;

            ServiceMaster ObjModel = null;
            try
            {
                ObjModel = new ServiceMasterDal().SelectByID(request.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            ObjModel.IsSubscriptionAllow = request.IsSubscriptionAllow;
            int Result = -1;
            try
            {
                Result = new ServiceMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
            {
                return Message.ServiceMessage.UpdateFail;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal List<Response.Service.ViewModel> SelectBBPSServices()
        {
            List<Response.Service.ViewModel> ReturnList = null;
            try
            {
                ReturnList = new ServiceMasterDal().SelectBBPSServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }

        internal List<Response.Service.ViewModel> SelectSubscriptionAllowedServices()
        {
            List<Response.Service.ViewModel> ReturnList = null;
            try
            {
                ReturnList = new ServiceMasterDal().SelectSubscriptionAllowedServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }
        internal List<Response.Service.ViewModel> SelectRangeWiseServices()
        {
            List<Response.Service.ViewModel> ReturnList = null;
            try
            {
                ReturnList = new ServiceMasterDal().SelectRangeWiseServices();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }
        internal List<ViewModel> SelectServiceMoneyTransferAndUPITransfer()
        {
            List<ViewModel> list = null;
            try
            {
                list = new ServiceMasterDal().SelectServiceMoneyTransferAndUPITransfer();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return list;
            }

            return list;
        }

        internal List<Response.Service.ViewModel> SelectSubscriptionServices()
        {
            List<Response.Service.ViewModel> ReturnList = null;
            List<Response.Service.ViewModel> BBPSList = null;

            var hardcodedServices = new List<Response.Service.ViewModel>
            {
                  new Response.Service.ViewModel { ServiceName = "BBPS", ServiceID = -999 },                 
            };

            try
            {
                ReturnList = new ServiceMasterDal().SelectSubscriptionServices();               
                ReturnList.AddRange(hardcodedServices);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }
    }
}