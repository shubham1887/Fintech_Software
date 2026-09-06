using Project.Model;
using System.Linq;

namespace Project.DAL
{
    public class Archive_TransactionMasterDal
    {
        internal void BulkInsert(int DelectDOCInt, int fromDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var dataToInsert = from row in context.TransactionMaster
                                   where row.DOCInt >= fromDOCInt
                                   && row.DOCInt <= DelectDOCInt
                                   orderby row.TransactionMasterID descending
                                   select new Archive_TransactionMaster
                                   {
                                       TransactionMasterID = row.TransactionMasterID,
                                       UserID = row.UserID,
                                       ServiceID = row.ServiceID,
                                       OperatorID = row.OperatorID,
                                       InboxID = row.InboxID,
                                       WalletID = row.WalletID,
                                       ConsumerNumber = row.ConsumerNumber,
                                       Amount = row.Amount,
                                       DebitAmount = row.DebitAmount,
                                       OpeningBalance = row.OpeningBalance,
                                       ClosingBalance = row.ClosingBalance,
                                       Status = row.Status,
                                       Description = row.Description,
                                       ProviderID = row.ProviderID,
                                       ProviderOpeningBalance = row.ProviderOpeningBalance,
                                       ProviderCommission = row.ProviderCommission,
                                       ProviderClosingBalance = row.ProviderClosingBalance,
                                       ProviderRequestID = row.ProviderRequestID,
                                       ReferenceNo = row.ReferenceNo,
                                       APIUserRequestID = row.APIUserRequestID,
                                       ProviderDOCNo = row.ProviderDOCNo,
                                       ProviderRefNo = row.ProviderRefNo,
                                       AdminGST = row.AdminGST,
                                       RetailerTDS = row.RetailerTDS,
                                       DistributerTDS = row.DistributerTDS,
                                       SuperDistributerTDS = row.SuperDistributerTDS,
                                       RetailerGST = row.RetailerGST,
                                       DistributerGST = row.DistributerGST,
                                       SuperDistributerGST = row.SuperDistributerGST,
                                       Bill_CustomerMessageMobileNo = row.Bill_CustomerMessageMobileNo,
                                       RetailerCommission = row.RetailerCommission,
                                       DistributerCommission = row.DistributerCommission,
                                       SuperDistributerCommission = row.SuperDistributerCommission,
                                       CashBack = row.CashBack,
                                       HitBackCount = row.HitBackCount,
                                       //LapuNo = row.LapuNo,
                                       Bill_Optional1 = row.Bill_Optional1,
                                       Bill_Optional2 = row.Bill_Optional2,
                                       Bill_Optional3 = row.Bill_Optional3,
                                       Bill_Optional4 = row.Bill_Optional4,
                                       Bill_Optional5 = row.Bill_Optional5,
                                       Bill_Optional6 = row.Bill_Optional6,
                                       DMT_SenderID = row.DMT_SenderID,
                                       DMT_SenderName = row.DMT_SenderName,
                                       DMT_SenderNumber = row.DMT_SenderNumber,
                                       DMT_BeneficiaryID = row.DMT_BeneficiaryID,
                                       DMT_BeneficiaryName = row.DMT_BeneficiaryName,
                                       DMT_Bank = row.DMT_Bank,
                                       DMT_IFSC = row.DMT_IFSC,
                                       DMT_BeneficaryNameByBank = row.DMT_BeneficaryNameByBank,
                                       DMT_PaymentType = row.DMT_PaymentType,
                                       DMT_BillNumber = row.DMT_BillNumber,
                                       DMT_OriginalAmount = row.DMT_OriginalAmount,
                                       DMT_StatusCode = row.DMT_StatusCode,
                                       IsDispute = row.IsDispute,
                                       Location = row.Location,
                                       SimCardDetails = row.SimCardDetails,
                                       Through = row.Through,
                                       IPAddress = row.IPAddress,
                                       Response = row.Response,
                                       Circle = row.Circle,
                                       DistributerUserID = row.DistributerUserID,
                                       SuperDistributerUserID = row.SuperDistributerUserID,
                                       ComplaintStatus = row.ComplaintStatus,
                                       AdminCommission = row.AdminCommission,
                                       CardType = row.CardType,
                                       StateID = row.StateID,
                                       IsROffer = row.IsROffer,
                                       IsOnlineDMT = row.IsOnlineDMT,
                                       IsProviderLowBalance = row.IsProviderLowBalance,
                                       ChangeStatus = row.ChangeStatus,
                                       ChangeStatusDOC = row.ChangeStatusDOC,
                                       AEPSResponseJSON = row.AEPSResponseJSON,
                                       PendingProviderStatus = row.PendingProviderStatus,
                                       AdminProfit = row.AdminProfit,
                                       ResponseType = row.ResponseType,
                                       ROfferCommission = row.ROfferCommission,
                                       RetailerCommissionHold = row.RetailerCommissionHold,
                                       DistributerCommissionHold = row.DistributerCommissionHold,
                                       SuperDistributerCommissionHold = row.SuperDistributerCommissionHold,
                                       SettlementAtProviderEnd = row.SettlementAtProviderEnd,
                                       BBPS_CustomerName = row.BBPS_CustomerName,
                                       DOC = row.DOC,
                                       UpdatedDOC = row.UpdatedDOC,
                                       DOCInt = row.DOCInt
                                   };

                var dataToCompare = from item in context.Archive_TransactionMaster
                                    where item.DOCInt >= fromDOCInt
                                    && item.DOCInt <= DelectDOCInt
                                    select item;

                var bulkInserData = dataToInsert.Where(x => !dataToCompare.Any(y => y.TransactionMasterID == x.TransactionMasterID));
                context.Archive_TransactionMaster.AddRange(bulkInserData);
                context.SaveChanges();
            }
        }

        internal bool IsValidforDeleteTransactionMaster(int DelectDOCInt, int fromDOCInt)
        {
            var context = new DatabaseContext();

            var TransaMain = context.TransactionMaster.Where(x => x.DOCInt >= fromDOCInt && x.DOCInt <= DelectDOCInt).ToList();
            var ArchiveMain = context.Archive_TransactionMaster.Where(x => x.DOCInt >= fromDOCInt && x.DOCInt <= DelectDOCInt).ToList();
            var table1NotInTable2 = TransaMain.GroupJoin(ArchiveMain,
                                          t1 => t1.TransactionMasterID,
                                          t2 => t2.TransactionMasterID,
                                          (t1, t2List) => new { t1, t2List })
                               .Where(x => !x.t2List.Any())
                               .Select(x => x.t1);
            foreach (var row in table1NotInTable2)
            {
                return false;
            }
            return true;
        }
    }
}