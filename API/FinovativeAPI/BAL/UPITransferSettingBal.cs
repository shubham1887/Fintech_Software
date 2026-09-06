using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Response.UPITransferSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UPITransferSettingBal
    {

        public List<ActiveUPIList> SelectActiveUPIList(out string failureMessage)
        {
            failureMessage = UPITransferSettingMessages.NotFound;
            List<ActiveUPIList> list = null;

            string UPITransferWithSenderFlag = new SettingMasterBal().getUPITransferWithSender();

            if (UPITransferWithSenderFlag.EqualsIgnoreCase("Yes"))
            {
                list = new UPITransferSettingDal().SelectUPITransferWithSender();
            }
            else
            {

                list = new UPITransferSettingDal().SelectActiveUPIList();

            }

            failureMessage = UPITransferSettingMessages.DeactiveUPITrasfer;
            if (list == null || list.Count <= 0) return list;

            failureMessage = StatusType.Ok;
            return list;
        }

        internal UPITransferSetting SelectUPITransferByName(string Name)
        {
            UPITransferSetting settings = null;
            try
            {
                settings = new UPITransferSettingDal().SelectUPITransferByName(Name);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }
    }
}