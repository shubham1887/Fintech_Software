import { PostAPI } from 'Components/Helper/Helpers';

export const SelectForRetailerDashborad = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForRetailerDashborad');
};

export const SelectPending = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectPendingTransaction');
};
export const SelectPendingProvider = async (request) => {
    return PostAPI(
        request,
        'TransactionReport/SelectPendingProviderTransaction'
    );
};
export const SelectDmtPending = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectDMTPending');
};

export const SelectForFilter = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForFilter');
};

export const SelectPayoutForFilter = async (request) => {
    return PostAPI(request, 'TransactionReport/Payout');
};
export const SelectForVendorPayReport = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForVendorPayReport');
};

export const SelectCashDepositForFilter = async (request) => {
    return PostAPI(request, 'TransactionReport/CashDeposit');
};

export const SelectLeadGenerationForFilter = async (request) => {
    return PostAPI(request, 'TransactionReport/LeadGeneration');
};

export const SelectDMTForReceipt = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectDMTForReceipt');
};

export const SelectDMTForReceiptForWeb = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectDMTForReceiptForWeb');
};

export const SelectForReceipt = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForReceipt');
};

export const SelectBus = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForBusPrint');
};

export const SelectCreditCardForReceipt = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectCreditCardForReceipt');
};

export const SelectAEPSForReceipt = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectAEPSForReceipt');
};

export const SelectMicroATMForReceipt = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectMicroATMForReceipt');
};

export const Search = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectByConsumerNumber');
};
export const SelectForAdminDashborad = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForAdminDashborad');
};

export const SelectSale = async (request) => {
    return PostAPI(request, 'TransactionReport/ServicewiseSale');
};
export const SelectLapuwise = async (request) => {
    return PostAPI(request, 'TransactionReport/LapuwiseReport');
};
export const SelectRoleCommission = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectRoleWithCommission');
};

export const SelectProviderFailuer = async (request) => {
    return PostAPI(request, 'TransactionReport/ProviderFailure');
};
export const ProviderSale = async (request) => {
    return PostAPI(request, 'TransactionReport/ProviderSale');
};

export const GSTsummary = async (request) => {
    return PostAPI(request, 'TransactionReport/GSTsummary');
};
export const LiveTransactions = async (request) => {
    return PostAPI(request, 'TransactionReport/LiveTransaction');
};
export const ManualSuccessFail = async (request) => {
    return PostAPI(request, 'TransactionReport/ManualSuccessFail');
};
export const SelectUserwiseSale = async (request) => {
    return PostAPI(request, 'TransactionReport/UserwiseSale');
};
export const SelectTopPerformer = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectTopPerformer');
};

export const SelectCMSOfflineDMT = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectCMSOfflineDMT');
};

export const SelectPendingWithFilter = async (request) => {
    return PostAPI(
        request,
        'TransactionReport/SelectPendingTransactionWithFilter'
    );
};
export const SelectDMTPendingWithFilter = async (request) => {
    return PostAPI(
        request,
        'TransactionReport/SelectDMTPendingTransactionWithFilter'
    );
};
export const SelectSuccessPayoutTransaction = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectSuccessPayoutTransaction');
};
export const SelectOperatorSale = async (request) => {
    return PostAPI(request, 'TransactionReport/OperatorSale');
};
export const SelectForLast10 = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForLast10');
};

export const SelectRangewiseDMT = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectRangewiseDMT');
};
export const SelectUserWiseDMTRangeReport = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectUserWiseDMTRangeReport');
};

export const SelectResendDMTTransaction = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectResendDMTTransaction');
};
export const MonthlySummarySelect = async (request) => {
    return PostAPI(request, 'TransactionReport/MonthlySummary');
};
export const SelectProviderFailureGraph = async (request) => {
    return PostAPI(request, 'TransactionReport/ProviderFailureGraph');
};
export const SelectForCreditCardFilter = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForCreditCardFilter');
};
export const MicroATM = async (request) => {
    return PostAPI(request, 'TransactionReport/MicroATM');
};
export const SelectForPOSReport = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForPOSReport');
};
export const SelectForCMSReport = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForCMSReport');
};
export const GSTOperatorWise = async (request) => {
    return PostAPI(request, 'TransactionReport/GSTOperatorWise');
};
export const PurchaseReport = async (request) => {
    return PostAPI(request, 'TransactionReport/PurchaseReport');
};
export const SelectForFilterByUserID = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForFilterByUserID');
};

export const SelectForFilterByOldAmtAdj = async (request) => {
    return PostAPI(request, 'UserAccounting/SelectForFilterByOldAmountAdjustment');
};
export const SelectForFilterByNewAmtAdj = async (request) => {
    return PostAPI(request, 'UserAccounting/SelectForFilterByNewAmountAdjustment');
};
export const SelectForFilterByOperatorID = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForFilterByOperatorID');
};
export const SelectForRechargeFilter = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForRechargeFilter');
};

export const SelectForBBPSFilter = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForBBPS');
};
export const SelectForRechargeCompactFilter = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForRechargeCompactFilter');
};

export const SelectForDMTFilter = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForDMTFilter');
};
export const SelectChangeStatus = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectChangeStatusByCallback');
};
export const SelectPastSuccessFail = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectPastSuccessFail');
};
export const SelectWalleTopUp = async (request) => {
    return PostAPI(request, 'TransactionReport/WalleTopUp');
};
export const SelectPanCard = async (request) => {
    return PostAPI(request, 'TransactionReport/PanCard');
};
export const SelectSuccessUPITransferTransaction = async (request) => {
    return PostAPI(
        request,
        'TransactionReport/SelectSuccessUPITransferTransaction'
    );
};

export const SelectUserwiseAccounting = async (request) => {
    return PostAPI(request, 'UserAccounting/UserwiseAccounting');
};
export const SelectManualSuccessFail= async (request) => {
    return PostAPI(request, 'UserAccounting/ManualSuccessFail');
};
export const SelectUPITransferForFilter = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectUPITransferForFilter');
};

export const SelectAccountOpening = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectAccountOpening');
};

export const SelectUPIPending = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectUPIPending');
};
export const SelectUPIPendingWithFilter = async (request) => {
    return PostAPI(
        request,
        'TransactionReport/SelectUPIPendingTransactionWithFilter'
    );
};

export const SelectRangewiseRecharge = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectRangewiseRecharge');
};
export const SelectForROfferRechargeFilter = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectForROfferRechargeFilter');
};


export const SelectOTTTransferForFilter = async (request) => {
    return PostAPI(request, 'TransactionReport/SelectOTTTransferForFilter');
};

export const SelectBusBookingTransferForFilter = async (request) => {
    return PostAPI(
        request,
        'TransactionReport/SelectBusBookingTransferForFilter'
    );
};
