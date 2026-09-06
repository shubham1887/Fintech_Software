using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.DMTSetting;
using Project.Response.DMTSettings;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class DMTSettingsBal
    {
        private readonly IMapper _mapper;

        public DMTSettingsBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        public DMTSettingsBal()
        {
        }

        internal DMTSetting SelectDMTByName(string Name)
        {
            DMTSetting settings = null;
            try
            {
                settings = new DMTSettingsDal().SelectDMTByName(Name);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }

        internal string Insert(InsertRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            DMTSetting ObjModel = new DMTSetting
            {
                Name = mainRequest.Name,
                UserName = mainRequest.UserName,
                Token = mainRequest.Token,
                WalletID = mainRequest.WalletID,
                CallBackURL = mainRequest.CallBackURL,
                IsActiveForWebsite = true,
                IsActive = mainRequest.IsActive,
                IsCCF = mainRequest.IsCCF
            };

            failureMessage = DMTSettingMessages.InsertFail;
            int Result = -1; ;
            try
            {
                Result = 1;
                //new DMTSettingsDal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return DMTSettingMessages.InsertSuccess;
        }

        private void DeatciveAll(int DMTSettingID)
        {
            try
            {
                List<DMTSetting> settings = new DMTSettingsDal().SelectActiveDMTForDeActive();
                if (settings.Count < 1)
                {
                    return;
                }

                if (settings.Count >= 2)
                {
                    bool has = settings.Any(x => x.DMTSettingID == DMTSettingID);

                    if (has == true)
                    {
                        return;
                    }
                    foreach (DMTSetting item in settings)
                    {
                        DMTSetting ObjDMTSetting = new DMTSettingsDal().SelectByPrimaryID(item.DMTSettingID);
                        ObjDMTSetting.IsActive = false;
                        new DMTSettingsDal().Update(ObjDMTSetting);
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            if (mainRequest.Name.EqualsIgnoreCase("JaiJuiDMT"))
            {
                failureMessage = DMTSettingMessages.InvalidActionURL;
                if (mainRequest.CallBackURL == null || mainRequest.CallBackURL.Length <= 0)
                    return failureMessage;
            }

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }

            failureMessage = DMTSettingMessages.NotFound;

            DMTSetting ObjModel;
            try
            {
                ObjModel = new DMTSettingsDal().SelectByPrimaryID(mainRequest.DMTSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }

            if (ObjModel == null) return failureMessage;

            ObjModel.DisplayName = mainRequest.DisplayName;
            ObjModel.IsActive = mainRequest.IsActive;
            ObjModel.Token = mainRequest.Token;
            ObjModel.WalletID = mainRequest.WalletID;
            ObjModel.CallBackURL = mainRequest.CallBackURL;
            ObjModel.UserName = mainRequest.UserName;
            ObjModel.IsNeft = mainRequest.IsNeft;
            ObjModel.IsCCF = mainRequest.IsCCF;
            ObjModel.SenderLimit = mainRequest.SenderLimit;
            ObjModel.SplitAmount = mainRequest.SplitAmount;
            ObjModel.MinAmount = mainRequest.MinAmount;
            ObjModel.Sequence = mainRequest.Sequence;
            ObjModel.Mode = mainRequest.Mode;
            ObjModel.IsExpressDMT = mainRequest.IsExpressDMT;

            if (ObjModel.IsActive == true)
            {
                DeatciveAll(ObjModel.DMTSettingID);
            }

            failureMessage = DMTSettingMessages.UpdateFail;
            int Result = -1; ;
            try
            {
                Result = new DMTSettingsDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return DMTSettingMessages.UpdateSuccess;
        }

        public bool ISActiveDMT(int ProviderID)
        {
            DMTSetting settings = new DMTSettingsBal().GetActiveDMTSetting(ProviderID);

            DMTSetting DMTActive = null;
            try
            {
                DMTActive = new DMTSettingsDal().SelectActiveDMTByName(settings.Name);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (DMTActive == null) return false;

            return true;
        }

        internal List<TransferModeModel> SelectTransferMode(SelectTransferModeRequest mainRequest, out string failureMessage)
        {
            failureMessage = DMTSettingMessages.NotFound;

            List<TransferModeModel> viewModel = new List<TransferModeModel>();

            DMTSetting settings;
            try
            {
                settings = new DMTSettingsDal().SelectDMTByName(mainRequest.ActiveDMT);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (settings == null) return viewModel;

            try
            {
                if (settings.IsNeft == false)
                {
                    return viewModel;
                }

                string TransferMode = "IMPS,NEFT";

                string[] mode = TransferMode.Split(",");

                foreach (var item in mode)
                {
                    TransferModeModel model = new TransferModeModel();
                    model.TransferMode = item;
                    viewModel.Add(model);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal List<ViewModel> Select(out string failureMessage)
        {
            failureMessage = DMTSettingMessages.NotFound;

            List<ViewModel> viewModel = null;

            List<DMTSetting> settings;
            try
            {
                settings = new DMTSettingsDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (settings == null) return viewModel;

            try
            {
                viewModel = _mapper.Map<List<ViewModel>>(settings);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return viewModel;

            failureMessage = DMTSettingMessages.NotFound;

            DMTSetting settings;
            try
            {
                settings = new DMTSettingsDal().SelectByPrimaryID(mainRequest.DMTSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (settings == null) return viewModel;
            try
            {
                viewModel = _mapper.Map<ViewModel>(settings);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return viewModel;
        }

        public List<Response.User.ActiveDMTList> SelectActiveDMTList()
        {
            List<Response.User.ActiveDMTList> list = null;
            try
            {
                list = new DMTSettingsDal().SelectActiveDMTList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        public string SelectActiveDMT()
        {
            string ActiveDMT = "";
            try
            {
                ActiveDMT = new DMTSettingsDal().SelectActiveDMT();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return ActiveDMT;
        }

        public List<Response.User.ActiveDMTList> SelectMultiActiveDMT(out string failureMessage)
        {
            List<Response.User.ActiveDMTList> list = null;
            try
            {
                list = new DMTSettingsDal().SelectActiveDMTList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (list == null || list.Count <= 0)
            {
                failureMessage = "DMT setting is not active, please contact to customer care";
                return list;
            }
            failureMessage = StatusType.Ok;
            return list;
        }

        public DMTSetting GetActiveDMTSetting(int providerID)
        {
            string ActiveDMT = DMTCode.getNameByID(providerID);

            DMTSetting settings = null;
            try
            {
                settings = new DMTSettingsDal().SelectDMTByName(ActiveDMT);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }


        internal DMTSetting SelectActivePayoutDMT(string DMTName)
        {
            DMTSetting settings = null;
            try
            {
                settings = new DMTSettingsDal().SelectPayoutDMT(DMTName);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }
    }
}