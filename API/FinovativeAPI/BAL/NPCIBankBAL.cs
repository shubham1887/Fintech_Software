using Project.DAL;
using System.Collections.Generic;
using Project.Response.NPCIBank;
using Project.Message;
using System;
using Project.Processing;
using Project.StaticModel;

namespace Project.BAL
{
    public class NPCIBankBAL
    {
        internal List<ViewModel> Select(out string failureMessage)
        {
            List<ViewModel> list = null;
            try
            {
                list = new NPCIBankDAL().Select();
                if (list == null)
                {
                    failureMessage = DMTBankMessage.NoBankFound;
                    return list;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return list;
            }
            failureMessage = StatusType.Ok;
            return list;

        }
    }
}
