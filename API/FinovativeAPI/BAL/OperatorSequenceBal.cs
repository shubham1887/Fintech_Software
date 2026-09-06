using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Sequence;
using Project.Response.Sequence;
using Project.Response.TransactionProviderHistory;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class OperatorSequenceBal
    {
        internal string Insert(InsertRequest mainRequest, int UserID, string Action)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }
            Task.Run(() =>
            {
                new RealProviderMappingBal().Insert(mainRequest.RealProviderID, mainRequest.OperatorID);
            });
            Task.Run(() =>
                {
                    new ViewProviderMappingBal().Insert(mainRequest.ViewProviderID, mainRequest.OperatorID);
                });
            try
            {
                new OperatorSequenceDal().Delete(mainRequest.OperatorID);

                int OperatorID = mainRequest.OperatorID;
                Parallel.ForEach(mainRequest.providerList, async (Provider) =>
                {
                    int result = await new OperatorSequenceDal().Insert(new OperatorSequence()
                    {
                        ProviderID = Provider.ProviderID,
                        OperatorID = mainRequest.OperatorID,
                        ServiceID = mainRequest.ServiceID,
                        Count = Provider.Count,
                    });

                    if (result <= 0) return;
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return StatusType.Ok;
        }

        internal OperatorSequenceResponse SelectByOperatorID(SelectByOperatorID selectByIDRequest, out string failureMessage)
        {
            failureMessage = selectByIDRequest.IsValidModalState();

            OperatorSequenceResponse SequenceResponse = null;
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return SequenceResponse;

            try
            {
                Response.Operator.ViewModel item = new OperatorMasterDAL().SelectDetailsByOperatorID(selectByIDRequest.OperatorID);

                SequenceResponse = new OperatorSequenceResponse();
                SequenceResponse.OperatorID = item.OperatorID;
                SequenceResponse.OperatorName = item.OperatorName;
                SequenceResponse.ServiceID = item.ServiceID;
                SequenceResponse.ServiceName = item.ServiceName;

                SequenceResponse.ProviderList = new OperatorSequenceDal().SelectProviderList(selectByIDRequest.OperatorID, item.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return SequenceResponse;
            }

            failureMessage = StatusType.Ok;
            return SequenceResponse;
        }

        internal List<OperatorSequenceResponse> Select(SelectByServiceID selectByServiceID, int UserID)
        {
            List<OperatorSequenceResponse> list = new List<OperatorSequenceResponse>();
            try
            {
                // List<Response.Operator.ViewModel> allOperator = new OperatorMasterBAL().SelectOperator();
                List<Response.Operator.ViewModelForDropDown> allOperator = new OperatorMasterDAL().SelectByServiceID(selectByServiceID.ServiceID, UserID);

                //var opts = new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 2.0)) };

                foreach (Response.Operator.ViewModelForDropDown item in allOperator)
                {
                    OperatorSequenceResponse seq = new OperatorSequenceResponse();
                    seq.Detail = $"{item.ServiceName}{Environment.NewLine}{item.OperatorName}";
                    seq.OperatorID = item.OperatorID;
                    seq.OperatorName = item.OperatorName;
                    seq.ServiceID = item.ServiceID;
                    seq.ServiceName = item.ServiceName;

                    seq.ProviderList = new OperatorSequenceDal().SelectProviderList(item.OperatorID, item.ServiceID);
                    // seq.RealProviderID = new RealProviderMappingDal().SelectRealProvider(item.OperatorID);
                    seq.ViewProviderID = new ViewProviderMappingDal().SelectViewProvider(item.OperatorID);

                    list.Add(seq);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        private List<ProviderList> SelectSequence(int OperatorID, int UserID, out bool isUserWise)
        {
            List<ProviderList> providerIDList = null;
            isUserWise = false;

            providerIDList = new OperatorSequenceUserWiseDal().SelectProviderList(OperatorID, UserID);
            if (providerIDList != null && providerIDList.Count >= 1)
            {
                isUserWise = true;
                return providerIDList;
            }

            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);

            if (user.ParentID == Company.AdminUserID) // if admin is parent of retiler/api user, then select default sequence
            {
                providerIDList = new OperatorSequenceDal().SelectByOperatorID(OperatorID);
                return providerIDList;
            }

            providerIDList = new OperatorSequenceUserWiseDal().SelectProviderList(OperatorID, user.ParentID);// dist
            if (providerIDList != null && providerIDList.Count >= 1)
            {
                return providerIDList;
            }

            user = new UserDAL().SelectByIDFromCache(user.ParentID);

            if (user == null)
            {
                providerIDList = new OperatorSequenceDal().SelectByOperatorID(OperatorID);
                return providerIDList;
            }

            if (user.ParentID == Company.AdminUserID) // if admin is parent of retiler/api user, then select default sequence
            {
                providerIDList = new OperatorSequenceDal().SelectByOperatorID(OperatorID);
                return providerIDList;
            }

            providerIDList = new OperatorSequenceUserWiseDal().SelectProviderList(OperatorID, user.ParentID);// super dist
            if (providerIDList != null && providerIDList.Count >= 1)
            {
                return providerIDList;
            }

            providerIDList = new OperatorSequenceDal().SelectByOperatorID(OperatorID);
            if (providerIDList != null && providerIDList.Count >= 1)
            {
                return providerIDList;
            }

            return providerIDList;
        }

        internal List<ProviderList> SelectSequenceByOperatorAndUser(int OperatorID, decimal Amount, int UserID, out bool isDefaultSequence)
        {
            isDefaultSequence = false;
            List<ProviderList> providerIDList = null;
            try
            {
                //setp 1 : check user wise sequence
                bool isUserWise = false;
                providerIDList = SelectSequence(OperatorID, UserID, out isUserWise);

                if (isUserWise == true) // if it is userwise, no need to check amount and count of recharge
                {
                    return providerIDList;
                }

                isDefaultSequence = true;
                //  setp 3 : check count(max 5) wise sequence
                providerIDList = SelectSequenceByCount(providerIDList, OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return providerIDList;
        }

        private List<ProviderList> SelectSequenceByAmont(List<ProviderList> list, int OperatorID, decimal Amount, out bool isQuantityFound)
        {
            List<ProviderList> providerIDList = new List<ProviderList>();
            isQuantityFound = false;
            try
            {
                foreach (ProviderList item in list)
                {
                    ProviderConfiguration ObjModel = new ProviderConfigurationDAL().SelectByProviderIDAndOperatorID(item.ProviderID, OperatorID);
                    bool isFound = IsAmountFound(ObjModel, Amount);
                    if (isFound == true)
                    {
                        isQuantityFound = true;
                        providerIDList.Insert(0, item);
                        continue;
                    }
                    providerIDList.Add(item);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return providerIDList;
        }

        private bool IsAmountFound(ProviderConfiguration ObjModel, decimal Amount)
        {
            if (ObjModel == null)
            {
                return false;
            }
            int amt = Convert.ToInt32(Amount);
            string qty = ObjModel.Quantity;

            if (qty == null)
            {
                return false;
            }
            if (qty == "0")
            {
                return false;
            }

            if (qty.Trim().Length <= 0)
            {
                return false;
            }

            string[] qtyArr = qty.Split(",");
            foreach (string item in qtyArr)
            {
                if (amt.ToString().EqualsIgnoreCase(item))
                {
                    return true;
                }

                if (isAmountInRange(qty, amt) == true)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isAmountInRange(string qty, int amt)
        {
            string[] qtyRangeArr = qty.Trim().Split('-', 2);
            if (qtyRangeArr.Length <= 1)
            {
                return false;
            }

            try
            {
                int low = Convert.ToInt32(qtyRangeArr[0].Trim());
                int high = Convert.ToInt32(qtyRangeArr[1].Trim());
                for (int i = low; i <= high; i++)
                {
                    if (i == amt)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return false;
        }

        private List<ProviderList> SelectSequenceByCount(List<ProviderList> list, int OperatorID)
        {
            List<ProviderList> providerIDList = new List<ProviderList>();
            try
            {
                int totalCount = list.Sum(item => item.Count);

                List<ProviderWithCount> historyList = new TransactionProviderHistoryBal().SelectTopNByOperatorID(OperatorID, totalCount);

                if (historyList == null || historyList.Count <= 0)
                    return list.OrderByDescending(x => x.Count).ToList();

                foreach (ProviderList item in list)
                {
                    int cnt = selectCountByProviderID(historyList, item.ProviderID); 
                    int diff = item.Count - cnt;

                    if (diff <= 0)
                    {
                        providerIDList.Add(item);
                        continue;
                    }
                    providerIDList.Insert(0, item);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return providerIDList;
        }

        private List<ProviderList> getListByUsage(List<ProviderList> list, int providerID)
        {
            List<ProviderList> providerList = new List<ProviderList>();
            List<ProviderList> providerNotFoundList = new List<ProviderList>();

            bool found = false;
            foreach (ProviderList item in list)
            {
                if (item.ProviderID != providerID && found == false)
                {
                    providerNotFoundList.Add(item);
                    continue;
                }
                found = true;
                providerList.Add(item);
            }

            foreach (ProviderList item in providerNotFoundList)
            {
                providerList.Add(item);
            }

            return providerList;
        }

        private ProviderList getProviderItem(List<ProviderList> list, int ProviderID)
        {
            foreach (ProviderList item in list)
            {
                if (item.ProviderID == ProviderID)
                {
                    return item;
                }
            }

            ProviderList itemMain = new ProviderList();
            itemMain.ProviderID = ProviderID;
            itemMain.Count = 1;

            return itemMain;
        }

        private int getMaxWorkDone(List<ProviderWithCount> historyList, int ProviderID)
        {
            int maxWorkDone = 0;

            //List<ProviderWithCount> newList = new List<ProviderWithCount>(historyList.Count);

            //historyList.ForEach((item) =>
            //{
            //    ProviderWithCount subitem = new ProviderWithCount();
            //    subitem.ProviderID = item.ProviderID;
            //    subitem.Count = item.Count;
            //    newList.Add(subitem);
            //});

            //newList.RemoveAt(newList.Count - 1);

            foreach (ProviderWithCount item in historyList)
            {
                if (item.ProviderID == ProviderID)
                {
                    maxWorkDone++;
                }
            }
            return maxWorkDone;
        }

        private int getWeightage(List<ProviderWithCount> historyList, int ProviderID, int maxWork, int Weightage)
        {
            int maxCount = 0;
            bool found = false;
            foreach (ProviderWithCount item in historyList)
            {
                if (item.ProviderID != ProviderID && found == false)
                {
                    int maxWorkDone = getMaxWorkDone(historyList, ProviderID);
                    if (maxWorkDone == maxWork)
                    {
                        Weightage += historyList.Count;
                        return Weightage;
                    }

                    Weightage += 1;
                    return Weightage;
                }

                if (item.ProviderID == ProviderID)
                {
                    maxCount++;
                    found = true;
                }
            }

            if (maxCount != maxWork)
            {
                Weightage += 1;
                return Weightage;
            }

            if (maxCount == maxWork && historyList[0].ProviderID == ProviderID)
            {
                Weightage += 50;
                return Weightage;
            }

            return Weightage;
        }

        private int selectCountByProviderID(List<ProviderWithCount> historyList, int ProviderID)
        {
            ProviderWithCount response = null;
            response = (from item in historyList
                        where item.ProviderID == ProviderID
                        select item).FirstOrDefault();

            if (response == null)
            {
                response = new ProviderWithCount();
                response.ProviderID = ProviderID;
                response.Count = 0;
            }
            return response.Count;
        }

        internal bool CheckSetDefultAndUserwiseSequence(int ProviderID)
        {
            List<OperatorSequence> providerIDList = null;
            List<OperatorSequenceUserWise> UserwiseList = null;

            providerIDList = new OperatorSequenceDal().SelectByProviderID(ProviderID);

            if (providerIDList.Count <= 0 || providerIDList == null)
            {
                UserwiseList = new OperatorSequenceUserWiseDal().SelectByProviderID(ProviderID);

                if (UserwiseList == null || UserwiseList.Count <= 0)
                    return true;
                return false;
            }

            return false;
        }

        internal List<ProviderList> SelectProviderForBulkSequence()
        {
            List<ProviderList> providerIDList = new List<ProviderList>();

            try
            {
                providerIDList = new OperatorSequenceDal().SelectProviderListForBulkUpdate();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return providerIDList;
        }

        internal string BulkUpdate(List<BulkUpdateRequest> mainRequest, out string failureMessge)
        {
            string Result = SequenceMessage.InsertSuccess;

            failureMessge = SequenceMessage.InvalidOperator;
            if (mainRequest == null || mainRequest.Count <= 0)
                return failureMessge;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
                return Result;

            int FailureCount = 0, SuccessCount = 0;

            foreach (BulkUpdateRequest request in mainRequest)
            {
                failureMessge = BulkUpdateSequence(request);

                if (!failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    FailureCount++;

                if (failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    SuccessCount++;
            }

            failureMessge = StatusType.Ok;
            if (FailureCount <= 0 && SuccessCount > 0) return Result;

            if (SuccessCount <= 0)
            {
                Result = SequenceMessage.InsertFail;
                return Result;
            }

            Result = SequenceMessage.BulkSequenceSuccessFail;
            Result = Result.Replace("[FailureCount]", FailureCount.ToString())
                           .Replace("[SuccessCount]", SuccessCount.ToString());
            return Result;
        }

        internal string BulkUpdateSequence(BulkUpdateRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }

            failureMessage = Message.UserMessage.UpdateFail;
            if (mainRequest.Criteria.EqualsIgnoreCase("RealAPI"))
            {
                new RealProviderMappingBal().Insert(mainRequest.RealProviderID, mainRequest.OperatorID);
                failureMessage = StatusType.Ok;
                return failureMessage;
            }
            if (mainRequest.Criteria.EqualsIgnoreCase("ViewAPI"))
            {
                new ViewProviderMappingBal().Insert(mainRequest.ViewProviderID, mainRequest.OperatorID);
                failureMessage = StatusType.Ok;
                return failureMessage;
            }
            if (mainRequest.Criteria.EqualsIgnoreCase("AddSequence"))
            {
                try
                {
                    int OperatorID = mainRequest.OperatorID;
                    // Parallel.ForEach(mainRequest.providerList, async (Provider) =>
                    foreach (ProviderList Provider in mainRequest.providerList)
                    {
                        OperatorSequence providerIDList = null;
                        providerIDList = new OperatorSequenceDal().SelectByOperatorIDAndProviderID(Provider.ProviderID, mainRequest.OperatorID);
                        if (providerIDList != null)
                        {
                            providerIDList.Count = mainRequest.Count;
                            new OperatorSequenceDal().Update(providerIDList);
                        }

                        if (providerIDList == null)
                        {
                            new OperatorSequenceDal().InsertForBulkUpdate(new OperatorSequence()
                            {
                                ProviderID = Provider.ProviderID,
                                OperatorID = mainRequest.OperatorID,
                                ServiceID = mainRequest.ServiceID,
                                Count = mainRequest.Count,
                            });
                        }
                    }//);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                failureMessage = StatusType.Ok;
                return failureMessage;
            }
            if (mainRequest.Criteria.EqualsIgnoreCase("RemoveSequence"))
            {
                try
                {
                    Parallel.ForEach(mainRequest.providerList, async (Provider) =>
                    {
                        new OperatorSequenceDal().DeleteByProvider(mainRequest.OperatorID, Provider.ProviderID);
                    });
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                failureMessage = StatusType.Ok;
                return failureMessage;
            }

            failureMessage = StatusType.Ok;
            return failureMessage;
        }
    }
}