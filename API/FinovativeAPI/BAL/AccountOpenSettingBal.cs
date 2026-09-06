using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.AccountOpenSetting;
using Project.Response.AccountOpenSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class AccountOpenSettingBal
    {
        private readonly IMapper _mapper;

        public AccountOpenSettingBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        public AccountOpenSettingBal()
        {
        }

        internal string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }

            failureMessage = AccountOpenSettingMessage.NotFound;

            AccountOpenSetting ObjModel;
            try
            {
                ObjModel = new AccountOpenSettingDal().SelectByPrimaryID(mainRequest.AccountOpenSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }

            if (ObjModel == null) return failureMessage;

            ObjModel = new AccountOpenSetting
            {
                AccountOpenSettingID = mainRequest.AccountOpenSettingID,
                Name = mainRequest.Name,
                IsActiveForWebsite = true,
                IsActive = mainRequest.IsActive,
                Token = mainRequest.Token,
                CallBackURL = mainRequest.CallBackURL,
                UserName = mainRequest.UserName,
                MerchantID = mainRequest.MerchantID,
                Sequence = mainRequest.Sequence,
                DisplayName = mainRequest.DisplayName,
                Mode = mainRequest.Mode,
            };

            if (ObjModel.IsActive == true)
            {
                DeatciveAll(ObjModel.AccountOpenSettingID);
            }

            failureMessage = AccountOpenSettingMessage.UpdateFail;
            int Result = -1; ;
            try
            {
                Result = new AccountOpenSettingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return AccountOpenSettingMessage.UpdateSuccess;
        }

        private void DeatciveAll(int AccountOpenSettingID)
        {
            try
            {
                List<AccountOpenSetting> settings = new AccountOpenSettingDal().SelectActiveAccountOpeningList();
                if (settings.Count < 1)
                {
                    return;
                }

                if (settings.Count >= 2)
                {
                    bool has = settings.Any(x => x.AccountOpenSettingID == AccountOpenSettingID);

                    if (has == true)
                    {
                        return;
                    }
                    foreach (AccountOpenSetting item in settings)
                    {
                        AccountOpenSetting ObjSetting = new AccountOpenSettingDal().SelectByPrimaryID(item.AccountOpenSettingID);
                        ObjSetting.IsActive = false;
                        new AccountOpenSettingDal().Update(ObjSetting);
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal List<ViewModel> Select(out string failureMessage)
        {
            failureMessage = AccountOpenSettingMessage.NotFound;

            List<ViewModel> viewModel = null;

            List<AccountOpenSetting> settings;
            try
            {
                settings = new AccountOpenSettingDal().Select();
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

            failureMessage = AccountOpenSettingMessage.NotFound;

            AccountOpenSetting settings;
            try
            {
                settings = new AccountOpenSettingDal().SelectByPrimaryID(mainRequest.AccountOpenSettingID);
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

        

        public List<ActiveList> SelectActiveList(out string failureMessage)
        {
            failureMessage = AccountOpenSettingMessage.NotFound;
            List<ActiveList> list = null;
            try
            {
                list = new AccountOpenSettingDal().SelectActiveList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = AccountOpenSettingMessage.DeactiveUPITrasfer;
            if (list == null || list.Count <= 0) return list;

            failureMessage = StatusType.Ok;
            return list;
        }

         
    }
}