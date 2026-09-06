using MplanCore.Processing;
using MplanCore.Request;
using MplanCore.Request.MobiKwik;
using MplanCore.Response;
using MplanCore.StaticModal;
using MPlanCore.Processing;
using Project.DAL;
using Project.Processing;
using Project.Requests.ROffer;
using Project.Response.PlanSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ROfferBal
    {
        internal MplanCore.Response.Records SelectPlan(SelectPlanRequest mainRequest, out string failureMessage)
        {
            MplanCore.Response.Records records = null;

            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return records;


            string Operator = new OperatorMasterDAL().SelectOperatorNameByOperatorID(mainRequest.OperatorID);

            ViewModel viewModel = null;
            try
            {
                viewModel = new PlanSettingDAL().SelectActivePlan();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Message.ROffer.NotActive;
            if (viewModel == null)
                return records;

            try
            {
                if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.MPlan))
                {
                    if (viewModel.Circle.Length <= 1)
                        viewModel.Circle = "Maharashtra Goa";

                    MplanCredentials.SetCredentials(viewModel.Token);
                    records = MPlanProcessing.SelectPlan(Operator, viewModel.Circle);
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.Ezytm))
                {
                    EzytmCredentials.SetCredentials(viewModel.UserID, viewModel.Token);
                    //records = EzytmProcessing.SelectPlan(Operator);
                    records = EzytmProcessing.SelectPlan(Operator, mainRequest.ConsumerNumber);
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.VastWebIndia))
                {
                    VastWebIndiaCredentials.SetCredentials(viewModel.UserID, viewModel.Token);
                    records = VastWebIndiaProcessing.SelectPlan(Operator);
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.Shighrapay))
                {
                    RechargeRequest input = new RechargeRequest();
                    input.opid = mainRequest.OperatorID.ToString();
                    input.mobile_number = mainRequest.ConsumerNumber;
                    input.api_token = viewModel.Token;
                    input.user_id = viewModel.UserID;
                    ShighraPayCredentials.SetCredentials(viewModel.UserID, viewModel.Token);
                    records = ShighrapayAPIProcessing.SelectPlan(input);
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.MobiKwik))
                {
                    PlanViewRequest input = new PlanViewRequest();
                    input.OperatorID = mainRequest.OperatorID.ToString();
                    records = MobiKwikProcessing.SelectPlan(input, Operator);
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.InfoAPI))
                {
                    InfoapiCredentials.SetCredentials(viewModel.Token);
                    records = InfoAPIProcessing.SelectPlan(Operator, mainRequest.ConsumerNumber);
                }

                if (records == null)
                {
                    failureMessage = Message.ROffer.PlanNotFound;
                    return records;
                }

                if (records.Message != null &&
                    (records.Message.EqualsIgnoreCase("Plan Not Available") || records.Message.EqualsIgnoreCase("Your plan is expired")))
                {
                    failureMessage = records.Message;
                    return records;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (records == null)
            {
                failureMessage = Message.ROffer.PlanNotFound;
                return records;
            }

            failureMessage = StatusType.Ok;
            return records;
        }

        internal List<MplanCore.Response.ROfferResponse.Record> SelectOffer(SelectOfferRequest mainRequest, out string failureMessage)
        {
            List<MplanCore.Response.ROfferResponse.Record> records = null;

            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return records;

            failureMessage = Message.ROffer.OfferNotFound;
            string Operator = new OperatorMasterDAL().SelectOperatorNameByOperatorID(mainRequest.OperatorID);

            ViewModel viewModel = null;
            try
            {
                viewModel = new PlanSettingDAL().SelectActivePlan();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = Message.ROffer.NotActive;

            if (viewModel == null)
                return records;

            try
            {
                if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.MPlan))
                {
                    string MPlanKey = viewModel.Token;
                    //string MPlanKey = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.MPlanKey));

                    MplanCredentials.SetCredentials(MPlanKey);

                    records = MPlanProcessing.SelectROffer(Operator, mainRequest.ConsumerNumber);

                    //new Logger().Info("MPlan SelectOffer", mainRequest.ConsumerNumber, records.Count + "");

                    //{"data": "[{\"rs\":null,\"desc\":\"Plan Not Available\",\"status\":null}]", "isAuthorized": true, "status": true}

                    if (records == null || records.Count <= 0 ||
                        records[0].desc.Trim().EqualsIgnoreCase("No data for ROffer") ||
                        records[0].desc.Trim().EqualsIgnoreCase("Plan Not Available") ||
                         records[0].desc.Trim().EqualsIgnoreCase("Your plan is expired"))
                    {
                        failureMessage = records[0].desc;
                        return records;
                    }
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.Ezytm))
                {
                    EzytmCredentials.SetCredentials(viewModel.UserID, viewModel.Token);

                    records = EzytmProcessing.SelectROffer(Operator, mainRequest.ConsumerNumber);

                    //new Logger().Info("MPlan SelectOffer", mainRequest.ConsumerNumber, records.Count + "");

                    //{"data": "[{\"rs\":null,\"desc\":\"Plan Not Available\",\"status\":null}]", "isAuthorized": true, "status": true}

                    if (records == null || records.Count <= 0 ||
                        records[0].desc.Trim().EqualsIgnoreCase("No data for ROffer") ||
                        records[0].desc.Trim().EqualsIgnoreCase("Plan Not Available"))
                    {
                        failureMessage = records[0].desc;
                        return records;
                    }
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.VastWebIndia))
                {
                    VastWebIndiaCredentials.SetCredentials(viewModel.UserID, viewModel.Token);

                    records = VastWebIndiaProcessing.SelectROffer(Operator, mainRequest.ConsumerNumber);

                    //new Logger().Info("MPlan SelectOffer", mainRequest.ConsumerNumber, records.Count + "");

                    //{"data": "[{\"rs\":null,\"desc\":\"Plan Not Available\",\"status\":null}]", "isAuthorized": true, "status": true}

                    if (records == null || records.Count <= 0 ||
                        records[0].desc.Trim().EqualsIgnoreCase("No data for ROffer") ||
                        records[0].desc.Trim().EqualsIgnoreCase("Plan Not Available"))
                    {
                        failureMessage = records[0].desc;
                        return records;
                    }
                }

                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.InfoAPI))
                {
                    InfoapiCredentials.SetCredentials(viewModel.Token);

                    records = InfoAPIProcessing.SelectROffer(Operator, mainRequest.ConsumerNumber);

                    if (records == null || records.Count <= 0 ||
                        records[0].desc.Trim().EqualsIgnoreCase("No data for ROffer") ||
                        records[0].desc.Trim().EqualsIgnoreCase("Plan Not Available"))
                    {
                        failureMessage = records[0].desc;
                        return records;
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return records;
            }

            if (records == null)
                return records;

            failureMessage = StatusType.Ok;
            return records;
        }

        internal DTHRecords SelectDTHPlan(SelectDTHPlanRequest mainRequest, out string failureMessage)
        {
            DTHRecords records = null;

            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return records;

            string Circle = "Maharashtra Goa";
            string Operator = new OperatorMasterDAL().SelectOperatorNameByOperatorID(mainRequest.OperatorID);
            failureMessage = Message.ROffer.OfferNotFound;

            ViewModel viewModel = null;
            try
            {
                viewModel = new PlanSettingDAL().SelectActivePlan();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = Message.ROffer.NotActive;
            if (viewModel == null)
                return records;

            try
            {
                if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.MPlan))
                {
                    string MPlanKey = viewModel.Token;
                    //string MPlanKey = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.MPlanKey));

                    MplanCredentials.SetCredentials(MPlanKey);
                    records = MPlanProcessing.SelectDTHPlan(Operator, Circle);
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.Ezytm))
                {
                    EzytmCredentials.SetCredentials(viewModel.UserID, viewModel.Token);
                    records = EzytmProcessing.SelectDTHPlan(Operator);
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.VastWebIndia))
                {
                    VastWebIndiaCredentials.SetCredentials(viewModel.UserID, viewModel.Token);
                    records = VastWebIndiaProcessing.SelectDTHPlan(Operator);
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.Shighrapay))
                {
                    DTHRequest input = new DTHRequest();
                    input.opid = mainRequest.OperatorID.ToString();
                    input.api_token = viewModel.Token;
                    input.user_id = viewModel.UserID;
                    ShighraPayCredentials.SetCredentials(viewModel.UserID, viewModel.Token);
                    records = ShighrapayAPIProcessing.SelectDTHPlan(input, Operator);
                }
                if (records.Message.EqualsIgnoreCase("Plan Not Available")
                   || records.Message.EqualsIgnoreCase("Your plan is expired"))
                {
                    failureMessage = records.Message;
                    return records;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return records;
            }

            failureMessage = StatusType.Ok;
            return records;
        }

        internal SelectCustomerInfoResponse SelectDTHCustInfo(SelectDTHCustInfoRequest mainRequest, out string failureMessage)
        {
            SelectCustomerInfoResponse records = null;
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return records;

            string Operator = new OperatorMasterDAL().SelectOperatorNameByOperatorID(mainRequest.OperatorID);
            failureMessage = Message.ROffer.OfferNotFound;

            ViewModel viewModel = null;
            try
            {
                viewModel = new PlanSettingDAL().SelectActivePlan();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = Message.ROffer.NotActive;
            if (viewModel == null)
                return records;

            try
            {
                if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.MPlan))
                {
                    string MPlanKey = viewModel.Token;
                    //string MPlanKey = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.MPlanKey));

                    MplanCredentials.SetCredentials(MPlanKey);
                    records = MPlanProcessing.SelectCustomerInfo(Operator, mainRequest.ConsumerNumber);
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.Ezytm))
                {
                    EzytmCredentials.SetCredentials(viewModel.UserID, viewModel.Token);
                    records = EzytmProcessing.SelectCustomerInfo(Operator, mainRequest.ConsumerNumber);
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.VastWebIndia))
                {
                    VastWebIndiaCredentials.SetCredentials(viewModel.UserID, viewModel.Token);
                    records = VastWebIndiaProcessing.SelectCustomerInfo(Operator, mainRequest.ConsumerNumber);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return records;
            }

            failureMessage = StatusType.Ok;
            return records;
        }

        internal HeavyRefreshResult HeavyRefresh(SelectDTHCustInfoRequest mainRequest, out string failureMessage)
        {
            HeavyRefreshResult response = null;
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            string Operator = new OperatorMasterDAL().SelectOperatorNameByOperatorID(mainRequest.OperatorID);
            failureMessage = Message.ROffer.OfferNotFound;

            ViewModel viewModel = null;
            try
            {
                viewModel = new PlanSettingDAL().SelectActivePlan();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Message.ROffer.NotActive;

            if (viewModel == null)
                return response;

            try
            {
                if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.MPlan))
                {
                    string MPlanKey = viewModel.Token;
                    //string MPlanKey = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.MPlanKey));

                    MplanCredentials.SetCredentials(MPlanKey);
                    response = MPlanProcessing.DTHHeavyRefresh(Operator, mainRequest.ConsumerNumber);
                }
                else if (viewModel.Name.EqualsIgnoreCase(PlanSettingCode.Ezytm))
                {
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }
    }
}