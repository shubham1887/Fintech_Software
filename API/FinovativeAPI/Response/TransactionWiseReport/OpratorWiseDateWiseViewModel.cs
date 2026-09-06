using Project.Processing;

namespace Project.Response.TransactionWiseReport
{
    public class OpratorWiseDateWiseViewModel
    {
        private string _DOC;
        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }
        private int _MobileSaleID;
        public int MobileSaleID
        {
            get => _MobileSaleID;
            set => _MobileSaleID = value;
        }
        private decimal _MobileSale;
        public decimal MobileSale
        {
            get => _MobileSale;
            set => _MobileSale = value;
        }

        private int _DTHSaleID;
        public int DTHSaleID
        {
            get => _DTHSaleID;
            set => _DTHSaleID = value;
        }
        private decimal _DTHSale;
        public decimal DTHSale
        {
            get => _DTHSale;
            set => _DTHSale = value;
        }

        private int _PaymentGatewaySaleID;
        public int PaymentGatewaySaleID
        {
            get => _PaymentGatewaySaleID;
            set => _PaymentGatewaySaleID = value;
        }
        private decimal _PaymentGatewaySale;
        public decimal PaymentGatewaySale
        {
            get => _PaymentGatewaySale;
            set => _PaymentGatewaySale = value;
        }

        private int _AepsSaleID;
        public int AepsSaleID
        {
            get => _AepsSaleID;
            set => _AepsSaleID = value;
        }

        private decimal _AepsSale;
        public decimal AepsSale
        {
            get => _AepsSale;
            set => _AepsSale = value;
        }

        private int _MAtmID;
        public int MAtmID
        {
            get => _MAtmID;
            set => _MAtmID = value;
        }
        private decimal _MAtm;
        public decimal MAtm
        {
            get => _MAtm;
            set => _MAtm = value;
        }

        private int _MoneyTransferID;
        public int MoneyTransferID
        {
            get => _MoneyTransferID;
            set => _MoneyTransferID = value;
        }
        private decimal _MoneyTransfer;
        public decimal MoneyTransfer
        {
            get => _MoneyTransfer;
            set => _MoneyTransfer = value;
        }
        private int _ElectricityID;
        public int ElectricityID
        {
            get => _ElectricityID;
            set => _ElectricityID = value;
        }
        private decimal _Electricity;
        public decimal Electricity
        {
            get => _Electricity;
            set => _Electricity = value;
        }
        private int _PostPaidID;
        public int PostPaidID
        {
            get => _PostPaidID;
            set => _PostPaidID = value;
        }
        private decimal _PostPaid;
        public decimal PostPaid
        {
            get => _PostPaid;
            set => _PostPaid = value;
        }

        private decimal _CreditCard;
        public decimal CreditCard
        {
            get => _CreditCard;
            set => _CreditCard = value;
        }

 

        private decimal _GeneralInsurance;
        public decimal GeneralInsurance
        {
            get => _GeneralInsurance;
            set => _GeneralInsurance = value;
        }

      
        private decimal _FasTag;
        public decimal FasTag
        {
            get => _FasTag;
            set => _FasTag = value;
        }


        private decimal _Broadband;
        public decimal Broadband
        {
            get => _Broadband;
            set => _Broadband = value;
        }

        private decimal _GasCylinderBooking;
        public decimal GasCylinderBooking
        {
            get => _GasCylinderBooking;
            set => _GasCylinderBooking = value;
        }

        private decimal _WaterBill;
        public decimal WaterBill
        {
            get => _WaterBill;
            set => _WaterBill = value;
        }

        private decimal _PipedGas;
        public decimal PipedGas
        {
            get => _PipedGas;
            set => _PipedGas = value;
        }

        private decimal _Payout;
        public decimal Payout
        {
            get => _Payout;
            set => _Payout = value;
        }

      

        private decimal _UPITransfer;
        public decimal UPITransfer
        {
            get => _UPITransfer;
            set => _UPITransfer = value;
        }



        private decimal _POS;
        public decimal POS
        {
            get => _POS;
            set => _POS = value;
        }

        private decimal _CMS;
        public decimal CMS
        {
            get => _CMS;
            set => _CMS = value;
        }



        private decimal _ExpressMoneyTransfer;
        public decimal ExpressMoneyTransfer
        {
            get => _ExpressMoneyTransfer;
            set => _ExpressMoneyTransfer = value;
        }

        private int _CreditCardID;
        public int CreditCardID
        {
            get => _CreditCardID;
            set => _CreditCardID = value;
        }

      

        private int _GeneralInsuranceID;
        public int GeneralInsuranceID
        {
            get => _GeneralInsuranceID;
            set => _GeneralInsuranceID = value;
        }

        private int _CMSID;
        public int CMSID
        {
            get => _CMSID;
            set => _CMSID = value;
        }

        private int _FasTagID;
        public int FasTagID
        {
            get => _FasTagID;
            set => _FasTagID = value;
        }

        private int _BroadbandID;
        public int BroadbandID
        {
            get => _BroadbandID;
            set => _BroadbandID = value;
        }

        private int _GasCylinderBookingID;
        public int GasCylinderBookingID
        {
            get => _GasCylinderBookingID;
            set => _GasCylinderBookingID = value;
        }

        private int _WaterBillID;
        public int WaterBillID
        {
            get => _WaterBillID;
            set => _WaterBillID = value;
        }

        private int _PipedGasID;
        public int PipedGasID
        {
            get => _PipedGasID;
            set => _PipedGasID = value;
        }

        private int _PayoutID;
        public int PayoutID
        {
            get => _PayoutID;
            set => _PayoutID = value;
        }

        private int _POSID;
        public int POSID
        {
            get => _POSID;
            set => _POSID = value;
        }
        

        private int _UPITransferID;
        public int UPITransferID
        {
            get => _UPITransferID;
            set => _UPITransferID = value;
        }

        private int _ExpressMoneyTransferID;
        public int ExpressMoneyTransferID
        {
            get => _ExpressMoneyTransferID;
            set => _ExpressMoneyTransferID = value;
        }


        private decimal _TotalService;
        public decimal TotalService
        {
            get => _TotalService;
            set => _TotalService = value;
        }
        public int ServiceID { get; internal set; }
        public int UserID { get; internal set; }
        private string _FirmName;
        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }
    }
}
