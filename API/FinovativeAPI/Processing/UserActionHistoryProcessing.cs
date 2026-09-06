using Project.DAL;
using Project.Requests.Transaction;
using Project.StaticModel;
using System;
using System.Linq;

namespace Project.Processing
{
    public class UserActionHistoryProcessing
    {
        public string GetDescriptionByAction(string Action, Object model)
        {
            string Description = "";
            switch (Action)
            {
                case UserAction.Sequence_Change:
                    Description = GenerateSequenceChangeMessage(ActionMessages.Sequence_Change, (Project.Requests.Sequence.InsertRequest)model);
                    break;

                case UserAction.Update_Status:
                    Description = GenerateUpdateStatusMessage(ActionMessages.Update_Status, (Project.Requests.Transaction.UpdateManualRequest)model);
                    break;
            };

            return Description;
        }

        private string GenerateUpdateStatusMessage(string message, UpdateManualRequest mainRequest)
        {
            string ConsumerNumber = new TransactionDAL().SelectConsumerNumberByTransactionMasterID(mainRequest.TransactionMasterID);
            message = message.Replace("[ConsumerNumber]", ConsumerNumber);
            message = message.Replace("[ReferenceNo]", mainRequest.ReferenceNo);
            message = message.Replace("[Status]", mainRequest.Status);
            return message;
        }

        private string GenerateSequenceChangeMessage(string message, Project.Requests.Sequence.InsertRequest mainRequest)
        {
            string OperatorName = new OperatorMasterDAL().SelectOperatorNameByOperatorID(mainRequest.OperatorID);
            string ProvicerNames = "";
            ProvicerNames = string.Join(",", mainRequest.providerList.Where(p => p.IsActive == true).Select(p => p.ProviderName));

            message = message.Replace("[OperatorName]", OperatorName);
            message = message.Replace("[ProvicerNames]", ProvicerNames);
            return message;
        }
    }
}