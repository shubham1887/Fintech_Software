using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.UISetting;
using Project.Response.UISetting;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UISettingsBal
    {
        private readonly IMapper _mapper;

        public UISettingsBal()
        {
        }

        public UISettingsBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        internal List<UISettingsViewModel> SelectAll()
        {
            List<UISettingMaster> list = null;
            List<UISettingsViewModel> report = null;
            try
            {
                list = new UISettingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list == null || list.Count <= 0)
                return report;

            report = new List<UISettingsViewModel>();
            foreach (UISettingMaster item in list)
            {
                report.Add(new UISettingsViewModel
                {
                    Key = item.Key,
                    Value = item.Value,
                });
            }
            return report;
        }

        internal List<ViewModel> Select(out string failureMessage)
        {
            List<ViewModel> response = new List<ViewModel>();
            List<UISettingMaster> list = null;
            try
            {
                list = new UISettingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = UISettingMessages.NotFound;
            if (list == null || list.Count <= 0)
                return response;
            try
            {
                response = _mapper.Map<List<ViewModel>>(list);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal ViewModel SelectByID(SelectRequest mainRequest, out string failureMessage)
        {
            failureMessage = UISettingMessages.NotFound;
            ViewModel response = new ViewModel();
            UISettingMaster dbResponse = null;
            try
            {
                dbResponse = new UISettingDal().SelectByID(mainRequest.UISettingMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (dbResponse == null)
                return response;
            try
            {
                response = _mapper.Map<ViewModel>(dbResponse);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UISettingMaster dbResponse = null;
            failureMessage = UISettingMessages.NotFound;

            try
            {
                dbResponse = new UISettingDal().SelectByID(mainRequest.UISettingMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (dbResponse == null)
                return "";
            failureMessage = UISettingMessages.UpdateFailure;
            dbResponse.Value = mainRequest.Value;
            dbResponse.Description = mainRequest.Description;
            int Result = -1;
            try
            {
                Result = new UISettingDal().Update(dbResponse);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (Result <= 0)
                return "";

            failureMessage = StatusType.Ok;
            return UISettingMessages.UpdateSuccess;
        }

        internal string UpdateAll(UpdateAllRequest mainRequest, out string failureMessage)
        {
            UISettingMaster dbResponse = null;

            failureMessage = UISettingMessages.NotFound;

            foreach (updateModel item in mainRequest.list)
            {
                try
                {
                    dbResponse = new UISettingDal().SelectByID(item.UISettingMasterID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (dbResponse == null)
                    return "";
                failureMessage = UISettingMessages.UpdateFailure;
                dbResponse.Value = item.Value;
                int Result = -1;
                try
                {
                    Result = new UISettingDal().Update(dbResponse);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            failureMessage = StatusType.Ok;
            return UISettingMessages.UpdateSuccess;
        }
    }
}