using CircleCore;
using CircleCore.Requests;
using CircleCore.Response;
using Project.BAL;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Requests.Sequence;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Project.Processing
{
    public class SequenceSelection
    {
        private List<ProviderList> FilterByListAndAmount(int operatorID, decimal Amount, List<Response.ProviderOperatorDenominationMapping.ViewModel> listInput)
        {
            List<ProviderList> denominationProviders = new List<ProviderList>();

            foreach (Response.ProviderOperatorDenominationMapping.ViewModel Provider in listInput)
            {
                int providerID = Provider.ProviderID;

                ProviderList newProvider = new ProviderList();
                newProvider.ProviderID = providerID;
                List<Response.ProviderOperatorDenominationMapping.ViewModel> MappingModelList = new List<Response.ProviderOperatorDenominationMapping.ViewModel>();
                if (Provider.IsStop)
                {
                    MappingModelList = new ProviderOperatorDenominationMappingDal().SelectIsStopByProviderAndOperatorID(providerID, operatorID);
                }
                else
                {
                    MappingModelList = new ProviderOperatorDenominationMappingDal().SelectByProviderAndOperatorID(providerID, operatorID);
                }
                if (MappingModelList == null || MappingModelList.Count <= 0) continue;

                foreach (Response.ProviderOperatorDenominationMapping.ViewModel MappingModel in MappingModelList)
                {
                    DenominationMaster ObjModel = new DenominationMasterDal().SelectByID(MappingModel.DenominationMasterID);

                    if (ObjModel == null) continue;

                    string DenominationRange = ObjModel.DenominationRange;

                    string[] CommaSeperatedArr = DenominationRange.Split(',');
                    for (int counter = 0; counter < CommaSeperatedArr.Length; counter++)
                    {
                        string CommaSeperatedStr = CommaSeperatedArr[counter];

                        if (CommaSeperatedStr.Trim().Length <= 0)
                        {
                            continue;
                        }

                        if (!CommaSeperatedStr.Contains("-"))
                        {
                            if (Convert.ToInt32(CommaSeperatedStr) == Amount)
                            {
                                denominationProviders.Add(newProvider);
                                break;
                            }
                        }

                        string[] DashSeperatedArr = CommaSeperatedStr.Split('-');
                        if (DashSeperatedArr.Length <= 1)
                        {
                            continue;
                        }

                        int From = Convert.ToInt32(DashSeperatedArr[0]);
                        int To = Convert.ToInt32(DashSeperatedArr[1]);
                        if (Amount >= From && Amount <= To)
                        {
                            denominationProviders.Add(newProvider);
                            break;
                        }
                    }
                }
            }

            return denominationProviders;
        }

        private List<ProviderList> GetListByDenomination(int operatorID, decimal Amount, List<ProviderList> providers)
        {
            List<ProviderList> denominationProviders = new List<ProviderList>();

            List<Response.ProviderOperatorDenominationMapping.ViewModel> allAllowedList = new ProviderOperatorDenominationMappingDal().SelectByOperatorID(operatorID);

            List<ProviderList> denominationProvidersOnlyAllowed = FilterByListAndAmount(operatorID, Amount, allAllowedList);

            foreach (ProviderList Provider in providers)
            {
                bool isFound = false;
                foreach (ProviderList dP in denominationProvidersOnlyAllowed)
                {
                    if (dP.ProviderID == Provider.ProviderID)
                    {
                        isFound = true;
                    }
                    if (isFound == true)
                        denominationProviders.Insert(0, Provider);
                }
                if (isFound == false)
                    denominationProviders.Add(Provider);
            }

            List<Response.ProviderOperatorDenominationMapping.ViewModel> allDisAllowedList = new ProviderOperatorDenominationMappingDal().SelectByOperatorIDWithStopDenomination(operatorID);

            List<ProviderList> denominationProvidersNotAllowed = FilterByListAndAmount(operatorID, Amount, allDisAllowedList);
            foreach (ProviderList Provider in providers)
            {
                bool isFound = false;
                foreach (ProviderList dP in denominationProvidersNotAllowed)
                {
                    if (dP.ProviderID == Provider.ProviderID)
                    {
                        isFound = true;
                        break;
                    }
                }

                if (isFound == true)
                {
                    denominationProviders.Remove(Provider);
                }
            }

            return denominationProviders;
        }

        internal HashSet<string> select(int OperatorID, decimal Amount, int UserID, string ConsumerNumber, out string failureMessage)
        {
            HashSet<string> list = new HashSet<string>();
            try
            {
                bool isDefaultSequence = false;
                List<ProviderList> providers = new OperatorSequenceBal().SelectSequenceByOperatorAndUser(OperatorID, Amount, UserID, out isDefaultSequence);

                if (isDefaultSequence == true) // means no user-wise seqqunce
                {
                    if (providers.Count <= 0)
                    {
                        failureMessage = TransactionMessage.sequenceNotFound;
                        return list;
                    }
                    providers = SelectWithCircleLogic(providers, OperatorID, ConsumerNumber);

                    providers = GetListByPendingCountLogic(OperatorID, providers);

                    if (providers.Count <= 0)
                    {
                        failureMessage = TransactionMessage.PendingCount;
                        return list;
                    }
                    providers = GetListByFailCountLogic(OperatorID, providers);

                    if (providers.Count <= 0)
                    {
                        failureMessage = TransactionMessage.FailCount;
                        return list;
                    }
                }
                providers = GetListByDenomination(OperatorID, Amount, providers);

                if (providers.Count <= 0)
                {
                    failureMessage = TransactionMessage.StopDenomination;
                    return list;
                }
                list = new HashSet<string>(providers.Select(o => o.ProviderID.ToString()).ToList());
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return list;
        }

        private List<ProviderList> GetListByPendingCountLogic(int OperatorID, List<ProviderList> providers)
        {
            List<ProviderList> PendingCountLogicProviders = new List<ProviderList>();

            ///initial all Add
            foreach (ProviderList Provider in providers)
            {
                PendingCountLogicProviders.Add(Provider);
            }

            foreach (ProviderList Provider in providers)
            {
                bool isFound = false;
                isFound = new TransactionDAL().IsSequentialPendingRechargeByProviderAndOperatorID(Provider.ProviderID, OperatorID);

                if (isFound == true)
                {
                    PendingCountLogicProviders.Remove(Provider);
                }
            }
            return PendingCountLogicProviders;
        }

        private List<ProviderList> GetListByFailCountLogic(int OperatorID, List<ProviderList> providers)
        {
            List<ProviderList> FailCountLogicProviders = new List<ProviderList>();

            ///initial all Add
            foreach (ProviderList Provider in providers)
            {
                FailCountLogicProviders.Add(Provider);
            }

            foreach (ProviderList Provider in providers)
            {
                bool isFound = new TransactionDAL().IsSequentialFailRechargeByProviderAndOperatorID(Provider.ProviderID, OperatorID);

                if (isFound == true)
                {
                    FailCountLogicProviders.Remove(Provider);
                }
            }
            return FailCountLogicProviders;
        }

        public CoreResponse GetCircleCode(string MobileNumber)
        {
            CoreResponse response = new CoreResponse();
            string URL = string.Empty;
            try
            {
                URL = new CircleAPIDal().SelectActiveURL();
            }
            catch (Exception)
            {
                return response;
            }
            if (URL == null || URL.Length <= 0)
            {
                response = new CoreResponse();
                response.Message = "Circle API url not set";
                return response;
            }

            GetCircleInfoRequest mainRequest = new GetCircleInfoRequest
            {
                Mobile = MobileNumber,
                Url = getURLWithReplace(URL, MobileNumber)
            };

            string failureMessage = "";

            try
            {
                response = CircleProcessing.GetCircleInfo(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (response == null)
            {
                response = new CoreResponse();
                response.Message = "Invalid response from circle api";
                return response;
            }
            return response;
        }

        private string getURLWithReplace(string url, string mobileNumber)
        {
            url = url.Replace("[Mobile]", mobileNumber);
            url = url.Replace("[mobile]", mobileNumber);

            url = url.Replace("[Number]", mobileNumber);
            url = url.Replace("[number]", mobileNumber);
            return url;
        }

        internal List<ProviderList> SelectWithCircleLogic(List<ProviderList> list, int OperatorID, string ConsumerNumber)
        {
            List<ProviderList> providerIDList = list;
            try
            {
                if (providerIDList == null) return providerIDList;

                providerIDList = SelectSequenceByCircleAPI(providerIDList, OperatorID, ConsumerNumber);
                return providerIDList;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return list;
        }

        private List<ProviderList> SelectSequenceByCircleAPI(List<ProviderList> list, int OperatorID, string ConsumerNumber)
        {
            List<ProviderList> finalProviderIDList = new List<ProviderList>();
            try
            {
                if (list == null)
                {
                    return list;
                }

                int CircleID = GetCircle(ConsumerNumber);

                if (CircleID <= 0)
                {
                    return list;
                }

                foreach (ProviderList item in list)
                {
                    List<ProviderOperatorCircleMaster> stateItemlist = new ProviderOperatorCircleMasterDal().SelectByProviderOperatorIDStateIDList(OperatorID, item.ProviderID);

                    if (stateItemlist.Count <= 0)
                    {
                        finalProviderIDList.Add(item);
                        continue;
                    }

                    bool flag = false;
                    foreach (ProviderOperatorCircleMaster stateitem in stateItemlist)
                    {
                        if (stateitem.StateID == CircleID)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                        finalProviderIDList.Insert(0, item);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return finalProviderIDList;
        }

        public string GetCircleWiseOpCode(int OperatorID, int ProviderID, int CircleID)
        {
            ProviderOperatorCircleMaster stateItem = new ProviderOperatorCircleMasterDal().SelectByProviderOperatorIDStateID(OperatorID, ProviderID, CircleID);

            if (stateItem == null)
            {
                return "";
            }
            return stateItem.Opcode;
        }

        private int GetCircle(string MobileNumber)
        {
            int circleCode = 0;
            string URL = new CircleAPIDal().SelectActiveURL();

            if (URL.Length <= 5)
            {
                return circleCode;
            }

            URL = getURLWithReplace(URL, MobileNumber);

            GetCircleInfoRequest mainRequest = new GetCircleInfoRequest
            {
                Mobile = MobileNumber,
                Url = URL
            };

            string failureMessage = "";
            CoreResponse response = null;
            try
            {
                response = CircleProcessing.GetCircleInfo(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (response == null)
                return circleCode;

            try
            {
                circleCode = Convert.ToInt32(response.CircleID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return circleCode;
        }
    }
}