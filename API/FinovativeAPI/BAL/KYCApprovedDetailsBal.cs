using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using Project.Response.KYCApprove;
using Project.Requests.KYC;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class KYCApprovedDetailsBal
    {

        internal string Insert(int UserID, int ApporvedUserID, string Remark)
        {
            string failureMessage;
            KYCApprovedDetails ObjModel = new KYCApprovedDetails
            {
                UserID = UserID,
                ApporvedUserID = ApporvedUserID,
                Remark = Remark,

            };
            try
            {
                int rowsAfftected = new KYCApprovedDal().Insert(ObjModel);

                if (rowsAfftected <= 0)
                {
                    return KYCMessage.InsertFailure;
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



        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new KYCApprovedDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }


    }
}