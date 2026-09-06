using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.PaymentModes;
using Project.Response.PaymentModes;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class PaymentModeBal
    {
        internal List<ViewModel> Select()
        {
            List<ViewModel> response = new List<ViewModel>();
            List<PaymentModeMaster> list = new List<PaymentModeMaster>();
            try
            {
                list = new PaymentModeDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            foreach (PaymentModeMaster p in list)
            {
                response.Add(new ViewModel
                {
                    PaymentModeMasterID = p.PaymentModeMasterID,
                    Type = p.Type
                });
            }

            return response;
        }

        internal List<ViewModel> SelectByReceiverID(SelectRequest selectByIDRequest, out string failureMessage)
        {
            List<ViewModel> response = new List<ViewModel>();

            failureMessage = selectByIDRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }

            List<PaymentModeMaster> list = new List<PaymentModeMaster>();
            try
            {
                list = new PaymentModeDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            CashCollectionMaster ObjModel = new CashCollectionMasterDal().SelectByUserID(selectByIDRequest.UserID);
            if (ObjModel == null)
            {
                new CashCollectionBal().InsertCashCollectionMaster(selectByIDRequest.UserID);
                ObjModel = new CashCollectionMasterDal().SelectByUserID(selectByIDRequest.UserID);
            }

            if (ObjModel == null)
            {
                failureMessage = "User not found";
                return response;
            }

            foreach (PaymentModeMaster p in list)
            {
                if (!(ObjModel.MaxOutstanding <= 0 && p.Type.EqualsIgnoreCase(PaymentModeType.Credit)))
                {
                    response.Add(new ViewModel
                    {
                        PaymentModeMasterID = p.PaymentModeMasterID,
                        Type = p.Type
                    });
                }
            }

            failureMessage = StatusType.Ok;
            return response;
        }
    }
}