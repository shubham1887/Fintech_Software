using Project.DAL;
using Project.StaticModel;
using System;

namespace Project.Processing
{
    public class ArchiveProcessing
    {
        public void ArchiveDataProcessing()
        {
            int ArchiveDays = getArchiveDays();
            int FromDays = ArchiveDays + 90;
            int DeleteDOCInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(-ArchiveDays)).ToString(Messages.DOCIntegerFomrat));
            int fromDOCInt = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(-FromDays)).ToString(Messages.DOCIntegerFomrat));
            try
            {
                Archive_TransactionMaster(DeleteDOCInt, fromDOCInt);
                Archive_TransactionDetails(DeleteDOCInt, fromDOCInt);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
            DeleteProcessing(DeleteDOCInt);
        }

        public void Archive_TransactionMaster(int DeleteDOCInt, int fromDOCInt)
        {
            try
            {
                //Insert to Archive_TransactionMaster
                new Archive_TransactionMasterDal().BulkInsert(DeleteDOCInt, fromDOCInt);
                bool IsValid = new Archive_TransactionMasterDal().IsValidforDeleteTransactionMaster(DeleteDOCInt, fromDOCInt);
                if (!IsValid)
                    return;
                new TransactionDAL().Delete(DeleteDOCInt, fromDOCInt);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
        }

        public void Archive_TransactionDetails(int DeleteDOCInt, int fromDOCInt)
        {
            try
            {
                //Insert to Archive_TransactionMaster
                new Archive_TransactionDetailsDal().BulkInsert(DeleteDOCInt, fromDOCInt);
                bool IsValid = new Archive_TransactionDetailsDal().IsValidforDeleteTransactionDetails(DeleteDOCInt, fromDOCInt);
                if (!IsValid)
                    return;
                new TransactionDetailsDAL().Delete(DeleteDOCInt, fromDOCInt);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
        }

        private int getArchiveDays()
        {
            int Days = Company.ArchiveDataInDays;
            try
            {
                if (Days <= 89)
                {
                    return 90;
                }
            }
            catch (Exception)
            {
                return Days;
            }

            return Days;
        }

        private void DeleteProcessing(int DeleteDOCInt)
        {
            new APIUserCallbackRequestResponseLogDal().Delete(DeleteDOCInt);
            new LapuTransactionDetailDal().Delete(DeleteDOCInt);
            new LoginHistoryDal().Delete(DeleteDOCInt);
            new NewsDAL().DeleteByDOCInt(DeleteDOCInt);
            new NotificationDal().Delete(DeleteDOCInt);
            new ProviderCallBackLogDal().Delete(DeleteDOCInt);
            new ProviderRequestResponseLogDal().Delete(DeleteDOCInt);
            new SentSmsDAL().Delete(DeleteDOCInt);
            new TransactionManualUpdateDal().Delete(DeleteDOCInt);
            new InboxDAL().Delete(DeleteDOCInt);
            new TrustableDeviceDal().Delete(DeleteDOCInt);
            new OTPMasterDal().Delete(DeleteDOCInt);
            new WrongPasswordHistoryDal().Delete(DeleteDOCInt);
            new SenderOTPDAL().Delete(DeleteDOCInt);
            new APIUserInitialRequestResponseLogDal().Delete(DeleteDOCInt);
            new PaymentGatewayLogDAL().Delete(DeleteDOCInt);
            new UserBalanceHistoryDAL().Delete(DeleteDOCInt);
            new TransactionProviderHistoryDal().Delete(DeleteDOCInt);
            new OperationalHistoryDal().Delete(DeleteDOCInt);
            new BulkMessageDAL().Delete(DeleteDOCInt);
        }
    }
}