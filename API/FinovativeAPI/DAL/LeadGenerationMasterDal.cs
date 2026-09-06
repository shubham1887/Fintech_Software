using Project.Model;
using Project.Processing;
using Project.Requests.LeadGenerationMaster;
using Project.Response.LeadGenerationMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Project.DAL
{
    public class LeadGenerationMasterDal
    {
        public static string Key = "_LeadGenerationMasterDal";
        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }
        internal int Insert(LeadGenerationMaster objModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.LeadGenerationMaster.Add(objModel);
            context.SaveChanges();
            return objModel.LeadGenerationMasterID;
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.LeadGenerationMaster
                        orderby setting.LeadGenerationMasterID descending
                        select new ViewModel
                        {
                            LeadGenerationMasterID = setting.LeadGenerationMasterID,
                            FormID = setting.FormID,
                            MerchantName = setting.MerchantName,
                            RetailerName = setting.RetailerName,
                            ConsumerName = setting.ConsumerName,
                            MobileNumber = setting.MobileNumber,
                            EmailID = setting.EmailID,
                            AadharNumber = setting.AadharNumber,
                            PanNumber = setting.PanNumber,
                            ConsumerType = setting.ConsumerType,
                            RequiredAmount = setting.RequiredAmount,
                            LoanType = setting.LoanType,
                            Status = setting.Status,
                            ApprovedAmount = setting.ApprovedAmount,
                            DOCInt = setting.DOCInt,
                            UserID = setting.UserID,
                            Gender = setting.Gender,
                            FromDate = setting.DOB.ToLongDateString(),
                            DOC = setting.DOC.LongFormatDate(),
                        }).ToList();
            }
        }


        public List<ViewModel> SelectReport(SelectRequest mainRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> mainList = Select();
            List<LoanTypes> loanList = new LoanTypesMasterDal().SelectLoanType();
            List<ConsumerTypes> consumerList = new ConsumerTypeMasterDal().SelectConsumerTypes();


            List<ViewModel> list = (from setting in mainList
                                    join loan in loanList on setting.LoanType equals loan.LoanTypeMasterID.ToString()
                                    join consumer in consumerList on setting.ConsumerType equals consumer.ConsumerTypeMasterID.ToString()
                                    where setting.DOCInt >= FromDate
                                    && setting.DOCInt <= ToDate
                                    && (mainRequest.UserID == 0 ? 2 > 1 : setting.UserID == mainRequest.UserID)
                                    select new ViewModel
                                    {
                                        LeadGenerationMasterID = setting.LeadGenerationMasterID,
                                        FormID = setting.FormID,
                                        MerchantName = setting.MerchantName,
                                        RetailerName = setting.RetailerName,
                                        ConsumerName = setting.ConsumerName,
                                        MobileNumber = setting.MobileNumber,
                                        EmailID = setting.EmailID,
                                        AadharNumber = setting.AadharNumber,
                                        PanNumber = setting.PanNumber,
                                        ConsumerType = consumer.ConsumerType ?? "",
                                        RequiredAmount = setting.RequiredAmount,
                                        LoanType = loan.LoanType ?? "",
                                        Status = setting.Status,
                                        ApprovedAmount = setting.ApprovedAmount,
                                        LoanTypeMasterID = loan.LoanTypeMasterID,
                                        Gender = setting.Gender,
                                        FromDate = setting.FromDate,
                                        DOC = setting.DOC,
                                    }).ToList();

            return list;
        }

        internal LeadGenerationMaster SelectByID(int LeadGenerationMasterID)
        {
            using var context = new DatabaseContext();
            return context.LeadGenerationMaster.Find(LeadGenerationMasterID);
        }

        internal int Update(LeadGenerationMaster model)
        {
            RemoveCache();
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.LeadGenerationMaster.Update(model);
                context.SaveChanges();
                result = model.LeadGenerationMasterID;
            }
            return result;
        }

        internal ViewModel SelectDuplicate(InsertRequest Lead)
        {
            List<ViewModel> list = Select();
            return (from lead in list
                    where lead.ConsumerName == Lead.ConsumerName
                        && lead.ConsumerType == Lead.ConsumerType && lead.MobileNumber == Lead.MobileNumber
                        && lead.EmailID == Lead.EmailID && lead.LoanType == Lead.LoanType && lead.Status == Lead.Status
                    select lead).FirstOrDefault();
        }
    }
}
