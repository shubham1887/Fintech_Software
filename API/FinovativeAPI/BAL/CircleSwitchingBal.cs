using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.CircleSwitching;
using Project.Response.CircleSwitching;

using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class CircleSwitchingBal
    {
        internal List<ViewModel> Select(SelectRequest mainRequest, int userID, out string failureMessage)
        {
            List<ViewModel> list = new List<ViewModel>();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return list;

            List<ZoneMaster> circles = new ZoneMasterDAL().Select();

            List<Project.Response.Provider.ConfigurationViewModel> operators = new ProviderConfigurationBAL().GetOperator(mainRequest.ProviderID, mainRequest.ServiceID);

            List<ViewModel> retrunList = new List<ViewModel>();

            //foreach (Project.Response.Provider.ConfigurationViewModel Op in operators)
            Parallel.ForEach(operators, Op =>
            {
                ViewModel objRet = new ViewModel();
                objRet.ProviderID = mainRequest.ProviderID;
                objRet.OperatorID = Op.OperatorID;
                objRet.Operator = Op.OperatorName;
                objRet.Circles = new List<OperatorCircleMaster>();

                bool isMobile = false;
                if (Op.ServiceID == (int)ServiceCode.Mobile)
                {
                    isMobile = true;
                }
                if (Op.ServiceID == (int)ServiceCode.Dth)
                {
                    isMobile = true;
                }

                if (Op.ServiceID == (int)ServiceCode.PostPaid)
                {
                    isMobile = true;
                }

                if (isMobile == false)
                {
                    // continue;
                    return;
                }

                ProviderOperatorCircleMaster newModel = new ProviderOperatorCircleMaster();

                newModel.ProviderID = mainRequest.ProviderID;
                newModel.OperatorID = Op.OperatorID;

                foreach (ZoneMaster circle in circles)
                {
                    newModel.StateID = circle.ZoneMasterID;

                    newModel.IsActive = false;

                    ProviderOperatorCircleMaster obj = new ProviderOperatorCircleMasterDal().SelectByProviderOperatorIDStateID(newModel.OperatorID,
                                                        newModel.ProviderID, newModel.StateID);

                    OperatorCircleMaster item = new OperatorCircleMaster();

                    item.CircleID = newModel.StateID;
                    item.CircleName = circle.Name;
                    item.IsActive = false;
                    item.Opcode = "";

                    if (obj == null)
                    {
                        try
                        {
                            int iid = new ProviderOperatorCircleMasterDal().Insert(new ProviderOperatorCircleMaster()
                            {
                                ProviderID = newModel.ProviderID,
                                OperatorID = newModel.OperatorID,
                                StateID = newModel.StateID,
                                Opcode = "",
                                IsActive = false
                            });

                            item.ProviderOperatorCircleMasterID = iid;
                        }
                        catch (Exception ex)
                        {
                            new Logger().Write(ex);
                        }
                    }
                    else
                    {
                        item.IsActive = obj.IsActive;
                        item.Opcode = obj.Opcode ?? "";
                        item.ProviderOperatorCircleMasterID = obj.ProviderOperatorCircleMasterID;
                    }

                    objRet.Circles.Add(item);
                }
                retrunList.Add(objRet);
            });

            failureMessage = StatusType.Ok;
            return retrunList;
        }

        internal string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            List<OperatorCircleMaster> operatorCircleMasters = mainRequest.Circles;

            Parallel.ForEach(operatorCircleMasters, obj =>
            {
                try
                {
                    int Result = new ProviderOperatorCircleMasterDal().Update(new ProviderOperatorCircleMaster
                    {
                        ProviderID = mainRequest.ProviderID,
                        OperatorID = mainRequest.OperatorID,
                        StateID = obj.CircleID,
                        IsActive = obj.IsActive,
                        Opcode = obj.Opcode ?? "",
                        ProviderOperatorCircleMasterID = obj.ProviderOperatorCircleMasterID
                    });
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            });

            /*foreach (OperatorCircleMaster obj in operatorCircleMasters)
            {
                try
                {
                    int Result = new ProviderOperatorCircleMasterDal().Update(new ProviderOperatorCircleMaster
                    {
                        ProviderID = mainRequest.ProviderID,
                        OperatorID = mainRequest.OperatorID,
                        StateID = obj.CircleID,
                        IsActive = obj.IsActive,
                        ProviderOperatorCircleMasterID = obj.ProviderOperatorCircleMasterID
                    });
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }*/

            failureMessage = StatusType.Ok;
            response = Message.CircleSwitchingMessages.UpdateSuccess;
            return response;
        }

        internal List<ViewModel> SelectCircleSwitching(int userID, out string failureMessage)
        {
            List<ViewModel> list = new List<ViewModel>();
            failureMessage = Messages.DataNotFound;
            try
            {
                list = new ProviderOperatorCircleMasterDal().SelectCircleSwitching();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            List<ViewModel> viewModel = null;

            if (list == null || list.Count <= 0) return viewModel;
            List<OperatorCircleMaster> objListCircle = new List<OperatorCircleMaster>();
            foreach (ViewModel item in list)
            {
                List<OperatorCircleMaster> circle = new ProviderOperatorCircleMasterDal().SelectCircles(item.OperatorID, item.ProviderID);
                item.Circles.AddRange(circle);
            }

            List<ViewModel> reslist = new List<ViewModel>();
            var distlist = list.Select(o => new { o.OperatorID, o.ProviderID }).Distinct().ToList();
            foreach (var item in distlist)
            {
                var singleData = list.Where(q => q.OperatorID == item.OperatorID && q.ProviderID == item.ProviderID).FirstOrDefault();
                reslist.Add(new ViewModel()
                {
                    ProviderOperatorCircleMasterID = singleData.ProviderOperatorCircleMasterID,
                    OperatorID = singleData.OperatorID,
                    Operator = singleData.Operator,
                    ProviderID = singleData.ProviderID,
                    ProviderName = singleData.ProviderName,
                    StateID = singleData.StateID,
                    Circles = singleData.Circles
                });
            }

            failureMessage = StatusType.Ok;
            return reslist;
        }

        internal string Delete(DeleteRequest deleteRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            List<ProviderOperatorCircleMaster> ObjModel;
            try
            {
                ObjModel = new ProviderOperatorCircleMasterDal().SelectByID(deleteRequest.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (ObjModel == null) return Message.DenominationMasterMessage.NotFound;

            failureMessage = Message.CircleSwitchingMessages.DeleleFail;
            int Result;
            try
            {
                Result = new ProviderOperatorCircleMasterDal().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (Result <= 0) return response;

            response = Message.CircleSwitchingMessages.DeleleSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }
    }
}