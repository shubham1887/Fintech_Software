using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.Provider;
using Project.Response.Provider;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class ProviderMasterBAL
    {
        internal string Insert(ProviderMasterInsertRequest InsertRequest)
        {
            string failureMessage = InsertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }

            ProviderMaster ObjModel = new ProviderMaster
            {
                ProviderName = InsertRequest.ProviderName,
                ProviderUrl = InsertRequest.ProviderUrl,
                IPAddress = InsertRequest.IPAddress,
                ResponseGroupID = InsertRequest.CopyFromProviderID,
                ProviderUniqueID = Guid.NewGuid().ToString(),
                IsRealAPI = InsertRequest.IsRealAPI,
                LoginID = InsertRequest.LoginID,
                Token = InsertRequest.Token,
                SupportNumber = InsertRequest.SupportNumber,
                LowBalanceAmount = InsertRequest.LowBalanceAmount,
                IsActive = true,
                IsHLR = false,
                IsDelete = false,
            };
            int rowsAffected = -1;
            try
            {
                rowsAffected = new ProviderMasterDAL().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0) return ProviderMessage.InsertProviderFail;
            return StatusType.Ok;
        }

        internal string ActiveDeactive(ProviderRequest ProviderRequest)
        {
            string failureMessage = ProviderRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderMaster ObjModel = null;
            try
            {
                ObjModel = new ProviderMasterDAL().SelectByID(ProviderRequest.ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.IsActive = ProviderRequest.IsActive;
            ObjModel.UpdatedDOC = DateTime.Now;
            int rowsAffected = -1;
            try
            {
                rowsAffected = new ProviderMasterDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0) return ProviderMessage.ProviderActiveDeactiveFail;

            return StatusType.Ok;
        }

        internal string Update(ProviderUpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }
            ProviderMaster ObjModel = null;
            try
            {
                ObjModel = new ProviderMasterDAL().SelectByID(updateRequest.ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.ProviderID = updateRequest.ProviderID;
            ObjModel.ProviderName = updateRequest.ProviderName;
            ObjModel.ProviderUrl = updateRequest.ProviderUrl;
            ObjModel.IPAddress = updateRequest.IPAddress;
            ObjModel.ResponseGroupID = updateRequest.CopyFromProviderID;
            ObjModel.IsRealAPI = updateRequest.IsRealAPI;
            ObjModel.LoginID = updateRequest.LoginID;
            ObjModel.Token = updateRequest.Token;
            ObjModel.SupportNumber = updateRequest.SupportNumber;
            ObjModel.LowBalanceAmount = updateRequest.LowBalanceAmount;
            ObjModel.UpdatedDOC = DateTime.Now;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new ProviderMasterDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return ProviderMessage.ProviderUpdateFail;

            return StatusType.Ok;
        }

        internal string Delete(ProviderRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }
            ProviderMaster ObjModel = null;

            try
            {
                ObjModel = new ProviderMasterDAL().SelectByID(deleteRequest.ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
                return ProviderMessage.ProviderNotFound; ;
            ObjModel.ProviderID = deleteRequest.ProviderID;
            ObjModel.IsDelete = true;
            ObjModel.IsActive = false;
            ObjModel.UpdatedDOC = DateTime.Now;
            int rowsAffected;
            try
            {
                rowsAffected = new ProviderMasterDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            new OperatorSequenceDal().DeleteSequenceByProviderID(deleteRequest.ProviderID);
            if (rowsAffected <= 0)

                return ProviderMessage.ProviderDeleteFail;

            return StatusType.Ok;
        }

        internal List<ViewModel> SelectProvider()
        {
            List<ViewModel> Provider = new List<ViewModel>();
            try
            {
                Provider = new ProviderMasterDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            foreach (ViewModel objmodel in Provider)
            {
                int RechargeBalance = Convert.ToInt32(objmodel.RechargeBalance);
                int DMTBalance = Convert.ToInt32(objmodel.DMTBalance);
                objmodel.BalanceDetails = $" Rech :{RechargeBalance} {Environment.NewLine} DMT : {DMTBalance}";
            }
            return Provider;
        }

        internal ViewModel SelectByID(ProviderRequest mainRequest)
        {
            ViewModel modelObj = null;
            ProviderMaster model;
            try
            {
                model = new ProviderMasterDAL().SelectByID(mainRequest.ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return modelObj;
            }

            if (model == null) return modelObj;

            modelObj = new ViewModel
            {
                ProviderID = model.ProviderID,
                ProviderName = model.ProviderName,
                ProviderUrl = model.ProviderUrl,
                IPAddress = model.IPAddress,
                CopyFromProviderID = model.ResponseGroupID,
                IsRealAPI = model.IsRealAPI,
                LoginID = model.LoginID,
                Token = model.Token,
                SupportNumber = model.SupportNumber,
                LowBalanceAmount = model.LowBalanceAmount,
            };
            return modelObj;
        }

        internal int SelectProviderIDByUniqueNo(string ProviderUniqueNo)
        {
            int ProviderID = 0;
            try
            {
                ProviderID = new ProviderMasterDAL().SelectProviderIDByUniqueNo(ProviderUniqueNo);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return ProviderID;
        }

        internal List<DropDownViewModel> SelectForDropDown(int UserID)
        {
            List<DropDownViewModel> Provider = new List<DropDownViewModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (!Util.isRoleAdminOrSupport(UserTypeID))
                return Provider;
            try
            {
                List<ViewModel> list = new ProviderMasterDAL().SelectOnlyActiveProviderID();

                foreach (ViewModel model in list)
                {
                    DropDownViewModel dropdown = new DropDownViewModel
                    {
                        ProviderID = model.ProviderID,
                        ProviderName = model.ProviderName
                    };
                    Provider.Add(dropdown);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return Provider;
        }

        internal string HLROnOff(ProviderRequest providerRequest)
        {
            string failureMessage = providerRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderMaster ObjModel = null;
            try
            {
                ObjModel = new ProviderMasterDAL().SelectByID(providerRequest.ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.IsHLR = providerRequest.IsHLR;
            ObjModel.UpdatedDOC = DateTime.Now;

            try
            {
                int rowsAffected = new ProviderMasterDAL().Update(ObjModel);
                if (rowsAffected <= 0) return ProviderMessage.HLRonoffFail;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        private static Dictionary<int, DateTime> _lastUpdateTimes = new Dictionary<int, DateTime>();

        private bool isValidToUpdateProviderBalance(int ProviderID)
        {
            if (!_lastUpdateTimes.ContainsKey(ProviderID))
            {
                return true;
            }

            if ((DateTime.Now - _lastUpdateTimes[ProviderID]).TotalMinutes >= 10)// in ten minute, only once update ProviderBalance
            {
                return true;
            }

            return false;
        }

        internal string UpdateProviderBalance(int ProviderID, string RechargeBalance, string DMTBalance)
        {
            ProviderMaster ObjModel = null;

            try
            {
                if (RechargeBalance == "0" && DMTBalance == "0")
                {
                    return "Both balances zero";
                }

                if (!new Validation().IsValidBalance(RechargeBalance))
                {
                    return "Recharge Balance invlaid";
                }

                if (!new Validation().IsValidBalance(DMTBalance))
                {
                    return "DMT Balance invlaid";
                }

                if (!isValidToUpdateProviderBalance(ProviderID))
                {
                    return "In ten minute, only once update Balance";
                }


                ObjModel = new ProviderMasterDAL().SelectByID(ProviderID);

                if (ObjModel == null)
                    return ProviderMessage.ProviderUpdateFail;

                //new Logger().Info("UpdateProviderBalance", ProviderID + ", RechargeBalance:" + RechargeBalance, "DMTBalance:" + DMTBalance);

                if (!new Validation().IsValidString(RechargeBalance))
                {
                    RechargeBalance = ObjModel.RechargeBalance.ToString();
                }

                if (!new Validation().IsValidString(DMTBalance))
                {
                    DMTBalance = ObjModel.DMTBalance.ToString();
                }

                RechargeBalance = cleanBalance(RechargeBalance);
                DMTBalance = cleanBalance(DMTBalance);

                if (RechargeBalance.EqualsIgnoreCase(Messages.MatcherParamNotFound))
                {
                    RechargeBalance = ObjModel.RechargeBalance.ToString();
                }

                if (DMTBalance.EqualsIgnoreCase(Messages.MatcherParamNotFound))
                {
                    DMTBalance = ObjModel.DMTBalance.ToString();
                }

                ObjModel.RechargeBalance = Convert.ToDecimal(RechargeBalance);
                ObjModel.DMTBalance = Convert.ToDecimal(DMTBalance);

                int rowsAffected = new ProviderMasterDAL().Update(ObjModel);
                if (rowsAffected <= 0) return ProviderMessage.ProviderUpdateFail;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            Task.Run(() => ProviderLowBalanceMessage(ProviderID, ObjModel.LowBalanceAmount, ObjModel.RechargeBalance, ObjModel.DMTBalance));

            return StatusType.Ok;
        }

        private void ProviderLowBalanceMessage(int ProviderID, int LowBalanceAmount, decimal RechargeBalance, decimal DMTBalance)
        {
            if (LowBalanceAmount <= 0) return;

            if (LowBalanceAmount <= Convert.ToInt32(RechargeBalance))
            {
                new MessageSelection().SendProviderLowBalanceMessage(ProviderID, RechargeBalance);
                return;
            }
            if (LowBalanceAmount <= Convert.ToInt32(DMTBalance))
            {
                new MessageSelection().SendProviderLowBalanceMessage(ProviderID, DMTBalance);
                return;
            }
        }

        private string cleanBalance(string bal)
        {
            try
            {
                string[] arr = bal.Split(" ");

                return arr[0];
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return "0";
        }

        internal BalanceViewModel CheckProviderBalance(ProviderRequest mainRequest, out string failureMessage)
        {
            BalanceViewModel response = new BalanceViewModel();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            DetailsViewModel ObjModel = null;
            if (!IsValidForAPIBalance(mainRequest.ProviderID))
            {
                try
                {
                    ObjModel = new ProviderDetailsDAL().GetBalanceURL(mainRequest.ProviderID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                if (ObjModel == null)
                {
                    failureMessage = ProviderMessage.BalanceUrlNotFound;
                    return response;
                }
            }
            ViewModel providerMaster = new ProviderMasterDAL().SelectByProviderID(mainRequest.ProviderID);
            ProviderBalanceProcessing BalanceProcessing = new ProviderBalanceProcessing();
            BalanceProcessing.ProviderBalanceCheck(providerMaster, ObjModel, response);
            failureMessage = StatusType.Ok;
            return response;
        }

        private bool IsValidForAPIBalance(int ProviderID)
        {
            if (ProviderID == Company.CashFreeProviderID)
                return true;
            if (ProviderID == Company.InstantPayPayoutProviderID)
                return true;
            if (ProviderID == Company.ToshaniBankProviderID)
                return true;
            if (ProviderID == Company.SafexPayProviderID)
                return true;
            if (ProviderID == Company.EkoPayoutProviderID)
                return true;
            if (ProviderID == Company.LevinProviderID)
                return true;
            if (ProviderID == Company.WaayuPayProviderID)
                return true;

            return false;
        }

        internal BalanceViewModel CheckAllProviderBalance()
        {
            BalanceViewModel response = new BalanceViewModel();

            List<Response.Provider.ViewModel> providerList = new ProviderMasterDAL().Select().Where(P => P.IsActive == true).ToList();
            foreach (ViewModel Provider in providerList)
            {
                DetailsViewModel ObjModel = null;
                    try
                    {
                        ObjModel = new ProviderDetailsDAL().GetBalanceURL(Provider.ProviderID);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                    if (ObjModel == null && Provider.ProviderID != Company.CashFreeProviderID)
                        continue;

                    ViewModel providerMaster = new ProviderMasterDAL().SelectByProviderID(Provider.ProviderID);
                    ProviderBalanceProcessing BalanceProcessing = new ProviderBalanceProcessing();
                    BalanceProcessing.ProviderBalanceCheck(providerMaster, ObjModel, response);       
            }
            return response;
        }

        internal List<DropDownViewModel> SelectForRealAPIDropDown()
        {
            List<DropDownViewModel> Provider = new List<DropDownViewModel>();
            List<ViewModel> list = null;
            try
            {
                list = new ProviderMasterDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list == null) return Provider;

            list = list.Where(l => l.IsRealAPI == true && l.IsActive == true).ToList();

            if (list == null) return Provider;

            foreach (ViewModel model in list)
            {
                DropDownViewModel dropdown = new DropDownViewModel
                {
                    ProviderID = model.ProviderID,
                    ProviderName = model.ProviderName
                };
                Provider.Add(dropdown);
            }
            return Provider;
        }

        internal List<DropDownViewModel> SelectForViewAPIDropDown()
        {
            List<DropDownViewModel> ViewProvider = new List<DropDownViewModel>();
            try
            {
                ViewProvider = new ProviderMasterDAL().SelectForViewAPIDropDown();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return ViewProvider;
        }

        internal List<DropDownViewModel> SelectDMTAsPayoutAPI()
        {
            List<DropDownViewModel> Provider = new List<DropDownViewModel>();
            try
            {
                List<ViewModel> list = new ProviderMasterDAL().Select();

                foreach (ViewModel model in list)
                {
                    DropDownViewModel dropdown = new DropDownViewModel
                    {
                        ProviderID = model.ProviderID,
                        ProviderName = model.ProviderName
                    };

                    bool IsPayoutDMT = FunctionalValidationProcessing.isPayoutDMT(dropdown.ProviderID);

                    if (IsPayoutDMT)
                        Provider.Add(dropdown);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return Provider;
        }

        internal List<DropDownViewModel> SelectDMTAPI(SelectByAllDMTRequest mainRequest)
        {
            List<DropDownViewModel> Provider = new List<DropDownViewModel>();
            try
            {
                List<DMTSetting> list = new DMTSettingsDal().SelectForProvider(mainRequest.UserTypeID);

                foreach (DMTSetting model in list)
                {
                    DropDownViewModel dropdown = new DropDownViewModel
                    {
                        ProviderName = model.Name,
                        ProviderID = DMTCode.getIDByName(model.Name)
                    };

                    Provider.Add(dropdown);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return Provider;
        }

        public int SelectByProviderUniqueID(string ProviderUniqueID)
        {
            ProviderMaster model = null;
            try
            {
                model = new ProviderMasterDAL().SelectByProviderUniqueID(ProviderUniqueID);
                if (model == null) return 0;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return 0;
            }

            return model.ProviderID;
        }

        public string SelectProviderUniqueID(string Name, int ServiceID)
        {
            int providerid = 0;

            if (ServiceID == (int)ServiceCode.MoneyTransfer)
            {
                providerid = DMTCode.getIDByName(Name);
            }

            if (ServiceID == (int)ServiceCode.UPITransfer)
            {
                providerid = UPITransferCode.getIDByName(Name);
            }

            ProviderMaster model = null;
            try
            {
                model = new ProviderMasterDAL().SelectActiveByID(providerid);
                if (model == null) return "";
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            return model.ProviderUniqueID;
        }
    }
}