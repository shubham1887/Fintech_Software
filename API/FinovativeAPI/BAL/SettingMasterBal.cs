using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.SettingMaster;
using Project.Response.SettingMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class SettingMasterBal
    {
        internal List<ViewModel> Select(int UserID)
        {
            List<ViewModel> settingMaster = new List<ViewModel>();

            try
            {
                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (UserTypeID != (int)UserType.Admin)
                {
                    return settingMaster;
                }
                //

                settingMaster = new SettingMasterDal().Select();

                foreach (ViewModel Model in settingMaster)
                {
                    Model.Key = new KeyIsPassword
                    {
                        KeyText = Model.KeyText,
                        Description = Model.Description
                    };

                    Model.Value = new ValueDescription
                    {
                        ValueText = Model.ValueText,
                        IsPassword = Model.IsPassword
                    };
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return settingMaster;
        }

        internal ViewModel SelectByID(SelectRequest mainRequest)
        {
            SettingMaster modelObj = new SettingMaster();
            ViewModel viewModel = null;
            try
            {
                modelObj = new SettingMasterDal().SelectByID(mainRequest.SettingMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (modelObj == null)
            {
                return viewModel;
            }
            viewModel.SettingMasterID = modelObj.SettingMasterID;
            viewModel.ValueText = modelObj.Value;
            viewModel.KeyText = modelObj.Key;
            viewModel.IsOn = modelObj.IsOn;
            return viewModel;
        }

        internal string Update(UpdateRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return failureMessage;
            }

            failureMessage = ServiceMessage.NotFound;

            SettingMaster model = new SettingMaster();

            try
            {
                model = new SettingMasterDal().SelectByID(mainRequest.SettingMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (model == null)
            {
                return failureMessage;
            }
            model.Value = mainRequest.Values;
            model.UpdatedDOC = DateTime.Now;
            int Result = -1;
            try
            {
                Result = new SettingMasterDal().Update(model);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
            {
                return SettingMessage.UpdateFail;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        internal string IsSecureOnOff(SelectRequest selectRequest)
        {
            string failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            SettingMaster ObjModel = null;
            try
            {
                ObjModel = new SettingMasterDal().SelectByID(selectRequest.SettingMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            ObjModel.IsSecure = selectRequest.IsSecure;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new SettingMasterDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return SettingMessage.UpdateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string SelectByUnSecureKey(SelectByKeyRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))

                return failureMessage;

            try
            {
                failureMessage = "Invalid key";
                if (mainRequest.Key.EqualsIgnoreCase(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.MPlanKey)))
                {
                    return failureMessage;
                }
                if (mainRequest.Key.EqualsIgnoreCase(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.EmailIDForSend)))
                {
                    return failureMessage;
                }
                if (mainRequest.Key.EqualsIgnoreCase(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.EmailPassword)))
                {
                    return failureMessage;
                }

                failureMessage = new SettingMasterDal().SelectByUnSecureKey(mainRequest.Key);
                if (failureMessage.Length <= 0)
                {
                    failureMessage = "Invalid key";
                    return failureMessage;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return failureMessage;
        }

        internal string SelectByKey(string key)
        {
            string value = "";
            try
            {
                value = new SettingMasterDal().SelectByKey(key);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return value;
        }

        internal string SelectByKeyWithDefaultValue(string key, string defaultValue, List<string> whiteList)
        {
            string valueInString = "";
            try
            {
                valueInString = new SettingMasterDal().SelectByKey(key);
            }
            catch (Exception)
            {
                valueInString = defaultValue;
            }
            if (!new Validation().IsValidString(valueInString))
            {
                valueInString = defaultValue;
                return valueInString;
            }
            bool found = false;

            foreach (string item in whiteList)
            {
                if (item.EqualsIgnoreCase(valueInString))
                {
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                return defaultValue;
            }
            return valueInString;
        }

        internal string SelectByKeyWithDefaultValueWithoutWhiteList(string key, string defaultValue)
        {
            string valueInString = "";
            try
            {
                valueInString = new SettingMasterDal().SelectByKey(key);
            }
            catch (Exception)
            {
                valueInString = defaultValue;
            }
            if (!new Validation().IsValidString(valueInString))
            {
                valueInString = defaultValue;
                return valueInString.Trim();
            }

            return valueInString.Trim();
        }

        internal int SelectByKeyWithIntegerDefaultValue(string key, int defaultValue)
        {
            int value;
            string vstr = "";
            try
            {
                vstr = new SettingMasterDal().SelectByKey(key);
                value = Convert.ToInt32(vstr);
            }
            catch (Exception)
            {
                value = defaultValue;
            }
            if (!new Validation().IsValidInt(value))
            {
                value = defaultValue;
                return value;
            }

            return value;
        }

        internal decimal SelectByKeyWithDecimalDefaultValue(string key, decimal defaultValue)
        {
            decimal value;
            string vstr = "";
            try
            {
                vstr = new SettingMasterDal().SelectByKey(key);
                value = Convert.ToDecimal(vstr);
            }
            catch (Exception)
            {
                value = defaultValue;
            }
            if (!new Validation().IsValidDecimal(value))
            {
                value = defaultValue;
                return value;
            }

            return value;
        }

        internal int SelectByKeyWithInteger(string key)
        {
            int value;
            string vstr = "";
            try
            {
                vstr = new SettingMasterDal().SelectByKey(key);
                value = Convert.ToInt32(vstr);
            }
            catch (Exception)
            {
                value = 0;
            }
            if (!new Validation().IsValidInt(value))
            {
                value = 0;
                return value;
            }

            return value;
        }

        internal string SelectByKeyWithoutDefault(string key)
        {
            string valueInString = "";
            try
            {
                valueInString = new SettingMasterDal().SelectByKey(key);
            }
            catch (Exception)
            {
            }
            if (!new Validation().IsValidString(valueInString))
            {
                return valueInString;
            }

            return valueInString;
        }

        internal int SelectMax_DMT_or_Payout_LimitByKey(string key)
        {
            int DefaultValue = 5000;
            int MaximumValue = 200000;
            int valueInInt = 0;
            string valueInString = "";
            try
            {
                valueInString = new SettingMasterDal().SelectByKey(key);
                if (!new Validation().IsValidString(valueInString))
                {
                    valueInInt = DefaultValue;
                    return valueInInt;
                }
                valueInInt = Convert.ToInt32(valueInString);
            }
            catch (Exception)
            {
                return DefaultValue;
            }

            if (Convert.ToInt32(valueInString) > MaximumValue)
            {
                valueInInt = MaximumValue;
                return valueInInt;
            }
            if (Convert.ToInt32(valueInString) < 5000)
            {
                valueInInt = DefaultValue;
                return valueInInt;
            }
            valueInInt = Convert.ToInt32(valueInString);
            return valueInInt;
        }

        internal int SelectMax_PasswordExpireDays()
        {
            int DefaultValue = 30;
            int MaximumValue = 1000;
            int valueInInt = DefaultValue;

            try
            {
                valueInInt = SelectByKeyWithInteger(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.PasswordExpireDays));
            }
            catch (Exception)
            {
                return DefaultValue;
            }

            if (valueInInt <= 0)
            {
                valueInInt = 5000;
                return valueInInt;
            }

            if (valueInInt > MaximumValue)
            {
                valueInInt = MaximumValue;
                return valueInInt;
            }

            return valueInInt;
        }

        public SelectOnlineDMTValueModel SelectOnlineDMTValue()
        {
            SelectOnlineDMTValueModel viewModel = new SelectOnlineDMTValueModel();
            viewModel.IsOnlineDMT = true;

            string OnlineDMT = new SettingMasterBal().SelectByKeyWithoutDefault(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.OnlineDMT));

            if (OnlineDMT.EqualsIgnoreCase("No"))
            {
                viewModel.IsOnlineDMT = false;
                return viewModel;
            }
            return viewModel;
        }

        public string getAEPSDailyAuthentication()
        {
            return new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.AEPSDailyAuthentication), "No");
        }

        public string getUPITransferWithSender()
        {
            return new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.UPITransferWithSender), "Yes");
        }

        public string getWalletToOther()
        {
            return new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ISWalletToOtherOTP), "Yes");
        }
        public string getW2OWithSenderWallet()
        {
            return new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.W2OWithSenderWallet), "No");
        }
        public string getFundReverseDirectToAdmin()
        {
            return new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.FundReverseDirectToAdmin), "No");
        }

        public string getAEPSAdharValidation()
        {
            return new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.AEPSAadharValidation), "No");
        }

        internal bool SelectByKeyWithoutWhiteList(string key)
        {
            bool returnvalue = false;

            string valueInString = "";
            try
            {
                valueInString = new SettingMasterDal().SelectByKey(key);

                if (valueInString.EqualsIgnoreCase("Yes"))
                {
                    returnvalue = true;
                }
            }
            catch (Exception)
            {
                return returnvalue;
            }
            if (!new Validation().IsValidString(valueInString))
            {
                return returnvalue;
            }

            return returnvalue;
        }

        internal bool GetFundRequestUplineWithAdmin()
        {
            bool returnvalue = false;

            string FundRequestUplineWithAdmin = "";
            try
            {
                List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

                FundRequestUplineWithAdmin = SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.FundRequestUplineWithAdmin), "No", whiteList);

                if (FundRequestUplineWithAdmin.EqualsIgnoreCase("Yes"))
                {
                    returnvalue = true;
                }
            }
            catch (Exception)
            {
                return returnvalue;
            }

            if (!new Validation().IsValidString(FundRequestUplineWithAdmin))
            {
                return returnvalue;
            }
            return returnvalue;
        }

        internal bool GetFundRequestUplineDFOSOrSDFOS()
        {

            string FundRequestUplineDFOSOrSDFOS = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.FundRequestUplineWithSDFOSandDFOS), "No");
            if (!FundRequestUplineDFOSOrSDFOS.EqualsIgnoreCase("Yes"))
                return false;
            return true;
        }


        internal bool GetTDSOn()
        {
            bool returnvalue = false;

            string TDSOn = "";
            try
            {
                List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

                TDSOn = SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.TDSOn), "No", whiteList);

                if (TDSOn.EqualsIgnoreCase("Yes"))
                {
                    returnvalue = true;
                }
            }
            catch (Exception)
            {
                return returnvalue;
            }

            if (!new Validation().IsValidString(TDSOn))
            {
                return returnvalue;
            }
            return returnvalue;
        }
        internal bool GetPerfomenceGraph()
        {
            bool returnvalue = false;

            string PerfomenceGraph = "";
            try
            {
                List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

                 PerfomenceGraph = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsProviderPerformanceGraph), "No");
  
                if (PerfomenceGraph.EqualsIgnoreCase("Yes"))
                {
                    returnvalue = true;
                }
            }
            catch (Exception)
            {
                return returnvalue;
            }
            if (!new Validation().IsValidString(PerfomenceGraph))
            {
                return returnvalue;
            }

            return returnvalue;
        }

        internal bool GetGSTOn()
        {
            bool returnvalue = false;

            string GSTOn = "";
            try
            {
                List<string> whiteList = new List<string>(new string[] { "Yes", "No" });

                GSTOn = SelectByKeyWithDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.GSTOn), "No", whiteList);

                if (GSTOn.EqualsIgnoreCase("Yes"))
                {
                    returnvalue = true;
                }
            }
            catch (Exception)
            {
                return returnvalue;
            }
            if (!new Validation().IsValidString(GSTOn))
            {
                return returnvalue;
            }

            return returnvalue;
        }

        public string UpdateOnlineDMTValue(UpdateOnlineDMTValueRequest mainRequest)
        {
            SettingMaster ObjModel = null;
            try
            {
                ObjModel = new SettingMasterDal().SelectByOnlineDMT();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return SettingMessage.RecordFound;
            }

            if (mainRequest.IsOnlineDMT == true)
                ObjModel.Value = "Yes";

            if (mainRequest.IsOnlineDMT == false)
                ObjModel.Value = "No";

            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new SettingMasterDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return SettingMessage.UpdateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal int getDMTSenderLimit()
        {
            int DefaultValue = 25000;
            int MaximumValue = 5000000;

            int SenderLimit = 0;

            string valueInString = "";
            try
            {
                valueInString = new SettingMasterDal().SelectByKey(SettingMasterKeys.DMTSenderLimit.ToString());

                if (!new Validation().IsValidString(valueInString))
                {
                    SenderLimit = DefaultValue;
                    return SenderLimit;
                }
                SenderLimit = Convert.ToInt32(valueInString);
            }
            catch (Exception)
            {
                return DefaultValue;
            }

            if (SenderLimit <= DefaultValue)
            {
                SenderLimit = DefaultValue;
                return SenderLimit;
            }

            if (SenderLimit > MaximumValue)
            {
                SenderLimit = MaximumValue;
                return SenderLimit;
            }

            return SenderLimit;
        }
    }
}