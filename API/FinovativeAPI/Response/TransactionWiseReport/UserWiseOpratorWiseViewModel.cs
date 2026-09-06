using Project.Processing;

namespace Project.Response.TransactionWiseReport
{
    public class UserWiseOpratorWiseViewModel
    {
        private int _UserID;
        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _ServiceID;
        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _FirmName;
        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _Role;
        public string Role
        {
            get => _Role.TrimIfNotNull();
            set => _Role = value.TrimIfNotNull();
        }

        private int _MobileSaleID;
        public int MobileSaleID
        {
            get => _MobileSaleID;
            set => _MobileSaleID = value;
        }

        private decimal _TodayMobileSale;
        public decimal TodayMobileSale
        {
            get => _TodayMobileSale;
            set => _TodayMobileSale = value;
        }

        private decimal _YesterdayMobileSale;
        public decimal YesterdayMobileSale
        {
            get => _YesterdayMobileSale;
            set => _YesterdayMobileSale = value;
        }

        private int _DTHSaleID;
        public int DTHSaleID
        {
            get => _DTHSaleID;
            set => _DTHSaleID = value;
        }

        private decimal _TodayDTHSale;
        public decimal TodayDTHSale
        {
            get => _TodayDTHSale;
            set => _TodayDTHSale = value;
        }

        private decimal _YesterdayDTHSale;
        public decimal YesterdayDTHSale
        {
            get => _YesterdayDTHSale;
            set => _YesterdayDTHSale = value;
        }

        private int _PaymentGatewaySaleID;
        public int PaymentGatewaySaleID
        {
            get => _PaymentGatewaySaleID;
            set => _PaymentGatewaySaleID = value;
        }

        private decimal _TodayPaymentGatewaySale;
        public decimal TodayPaymentGatewaySale
        {
            get => _TodayPaymentGatewaySale;
            set => _TodayPaymentGatewaySale = value;
        }

        private decimal _YesterdayPaymentGatewaySale;
        public decimal YesterdayPaymentGatewaySale
        {
            get => _YesterdayPaymentGatewaySale;
            set => _YesterdayPaymentGatewaySale = value;
        }

        private int _AepsSaleID;
        public int AepsSaleID
        {
            get => _AepsSaleID;
            set => _AepsSaleID = value;
        }

        private decimal _TodayAepsSale;
        public decimal TodayAepsSale
        {
            get => _TodayAepsSale;
            set => _TodayAepsSale = value;
        }

        private decimal _YesterdayAepsSale;
        public decimal YesterdayAepsSale
        {
            get => _YesterdayAepsSale;
            set => _YesterdayAepsSale = value;
        }

        private int _MAtmID;
        public int MAtmID
        {
            get => _MAtmID;
            set => _MAtmID = value;
        }

        private decimal _TodayMAtm;
        public decimal TodayMAtm
        {
            get => _TodayMAtm;
            set => _TodayMAtm = value;
        }

        private decimal _YesterdayMAtm;
        public decimal YesterdayMAtm
        {
            get => _YesterdayMAtm;
            set => _YesterdayMAtm = value;
        }

        private int _MoneyTransferID;
        public int MoneyTransferID
        {
            get => _MoneyTransferID;
            set => _MoneyTransferID = value;
        }

        private decimal _TodayMoneyTransfer;
        public decimal TodayMoneyTransfer
        {
            get => _TodayMoneyTransfer;
            set => _TodayMoneyTransfer = value;
        }

        private decimal _YesterdayMoneyTransfer;
        public decimal YesterdayMoneyTransfer
        {
            get => _YesterdayMoneyTransfer;
            set => _YesterdayMoneyTransfer = value;
        }

        private int _ElectricityID;
        public int ElectricityID
        {
            get => _ElectricityID;
            set => _ElectricityID = value;
        }

        private decimal _TodayElectricity;
        public decimal TodayElectricity
        {
            get => _TodayElectricity;
            set => _TodayElectricity = value;
        }

        private decimal _YesterdayElectricity;
        public decimal YesterdayElectricity
        {
            get => _YesterdayElectricity;
            set => _YesterdayElectricity = value;
        }

        private int _PostPaidID;
        public int PostPaidID
        {
            get => _PostPaidID;
            set => _PostPaidID = value;
        }

        private decimal _TodayPostPaid;
        public decimal TodayPostPaid
        {
            get => _TodayPostPaid;
            set => _TodayPostPaid = value;
        }

        private decimal _YesterdayPostPaid;
        public decimal YesterdayPostPaid
        {
            get => _YesterdayPostPaid;
            set => _YesterdayPostPaid = value;
        }

        private decimal _TodayCreditCard;
        public decimal TodayCreditCard
        {
            get => _TodayCreditCard;
            set => _TodayCreditCard = value;
        }

        private decimal _YesterdayCreditCard;
        public decimal YesterdayCreditCard
        {
            get => _YesterdayCreditCard;
            set => _YesterdayCreditCard = value;
        }

        private decimal _TodayGeneralInsurance;
        public decimal TodayGeneralInsurance
        {
            get => _TodayGeneralInsurance;
            set => _TodayGeneralInsurance = value;
        }

        private decimal _YesterdayGeneralInsurance;
        public decimal YesterdayGeneralInsurance
        {
            get => _YesterdayGeneralInsurance;
            set => _YesterdayGeneralInsurance = value;
        }

        private decimal _TodayFasTag;
        public decimal TodayFasTag
        {
            get => _TodayFasTag;
            set => _TodayFasTag = value;
        }

        private decimal _YesterdayFasTag;
        public decimal YesterdayFasTag
        {
            get => _YesterdayFasTag;
            set => _YesterdayFasTag = value;
        }

        private decimal _TodayBroadband;
        public decimal TodayBroadband
        {
            get => _TodayBroadband;
            set => _TodayBroadband = value;
        }

        private decimal _YesterdayBroadband;
        public decimal YesterdayBroadband
        {
            get => _YesterdayBroadband;
            set => _YesterdayBroadband = value;
        }

        private decimal _TodayPipedGas;
        public decimal TodayPipedGas
        {
            get => _TodayPipedGas;
            set => _TodayPipedGas = value;
        }

        private decimal _YesterdayPipedGas;
        public decimal YesterdayPipedGas
        {
            get => _YesterdayPipedGas;
            set => _YesterdayPipedGas = value;
        }

        private decimal _TodayPayout;
        public decimal TodayPayout
        {
            get => _TodayPayout;
            set => _TodayPayout = value;
        }

        private decimal _YesterdayPayout;
        public decimal YesterdayPayout
        {
            get => _YesterdayPayout;
            set => _YesterdayPayout = value;
        }

        private decimal _TodayUPITransfer;
        public decimal TodayUPITransfer
        {
            get => _TodayUPITransfer;
            set => _TodayUPITransfer = value;
        }

        private decimal _YesterdayUPITransfer;
        public decimal YesterdayUPITransfer
        {
            get => _YesterdayUPITransfer;
            set => _YesterdayUPITransfer = value;
        }

        private decimal _TodayPOS;
        public decimal TodayPOS
        {
            get => _TodayPOS;
            set => _TodayPOS = value;
        }

        private decimal _YesterdayPOS;
        public decimal YesterdayPOS
        {
            get => _YesterdayPOS;
            set => _YesterdayPOS = value;
        }

        private decimal _TodayCMS;
        public decimal TodayCMS
        {
            get => _TodayCMS;
            set => _TodayCMS = value;
        }

        private decimal _YesterdayCMS;
        public decimal YesterdayCMS
        {
            get => _YesterdayCMS;
            set => _YesterdayCMS = value;
        }

        private decimal _TodayExpressMoneyTransfer;
        public decimal TodayExpressMoneyTransfer
        {
            get => _TodayExpressMoneyTransfer;
            set => _TodayExpressMoneyTransfer = value;
        }

        private decimal _YesterdayExpressMoneyTransfer;
        public decimal YesterdayExpressMoneyTransfer
        {
            get => _YesterdayExpressMoneyTransfer;
            set => _YesterdayExpressMoneyTransfer = value;
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

        private decimal _PastTotalService;
        public decimal PastTotalService
        {
            get => _PastTotalService;
            set => _PastTotalService = value;
        }

        private decimal _TodayTotalService;
        public decimal TodayTotalService
        {
            get => _TodayTotalService;
            set => _TodayTotalService = value;
        }
    }
}
