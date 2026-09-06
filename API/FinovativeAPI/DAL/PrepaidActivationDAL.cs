using Project.Model;
using Project.Processing;
using Project.Requests.PrepaidActivation;
using Project.Response.PrepaidActivation;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class PrepaidActivationDAL
    {
        public int Insert(PrepaidActivation ObjModel)
        {
            using var context = new DatabaseContext();
            context.PrepaidActivation.Add(ObjModel);
            return context.SaveChanges();
        }

        internal int Update(PrepaidActivation objModel)
        {
            using var context = new DatabaseContext();
            context.PrepaidActivation.Update(objModel);
            return context.SaveChanges();
        }

        internal PrepaidActivation SelectByID(int PrepaidActivationID)
        {
            using var context = new DatabaseContext();
            return context.PrepaidActivation.Find(PrepaidActivationID);
        }

        internal List<ViewModel> Select(int UserID)
        {
            List<ViewModel> list = new List<ViewModel>();
            using var context = new DatabaseContext();
            list = (from activation in context.PrepaidActivation
                    join user in context.UserMaster on activation.UserID equals user.UserID
                    join op in context.OperatorMaster on activation.OperatorID equals op.OperatorID
                    where activation.UserID == UserID
                    orderby activation.PrepaidActivationID descending
                    select new ViewModel
                    {
                        PrepaidActivationID = activation.PrepaidActivationID,
                        UserName = user.FirmName + " " + user.MobileNumber,
                        OperatorName = op.OperatorName,
                        ActivationType = activation.ActivationType,
                        AlternateNumber = activation.AlternateNumber,
                        Name = activation.Name,
                        Address = activation.Address,
                        Pincode = activation.Pincode,
                        Status = activation.Status,
                        UserID = activation.UserID,
                        Date = activation.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                    }).Take(10).ToList();
            return list;
        }

        internal List<ViewModel> SelectFilter(SelectRequest selectRequest, int UserID)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(selectRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(selectRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> list = new List<ViewModel>();
            using var context = new DatabaseContext();
            list = (from activation in context.PrepaidActivation
                    join user in context.UserMaster on activation.UserID equals user.UserID
                    join op in context.OperatorMaster on activation.OperatorID equals op.OperatorID
                    where activation.DOCInt >= From
                       && activation.DOCInt <= To
                    orderby activation.PrepaidActivationID descending
                    select new ViewModel
                    {
                        PrepaidActivationID = activation.PrepaidActivationID,
                        UserName = user.FirmName + " " + user.MobileNumber,
                        OperatorName = op.OperatorName,
                        ActivationType = activation.ActivationType,
                        AlternateNumber = activation.AlternateNumber,
                        Name = activation.Name,
                        Address = activation.Address,
                        Pincode = activation.Pincode,
                        Status = activation.Status,
                        UserID = activation.UserID,
                        Date = activation.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                    }).ToList();
            if (UserID != Company.AdminUserID)
            {
                list = (from item in list
                        where item.UserID == UserID
                        select item).ToList();
            }
            if (!selectRequest.Status.EqualsIgnoreCase("All"))
            {
                list = (from item in list
                        where item.Status == selectRequest.Status
                        select item).ToList();
            }
            return list;
        }
    }
}