import { PostAPI } from 'Components/Helper/Helpers';

export const Recharge = async (request) => {
    return PostAPI(request, 'Transaction/Recharge');
};

export const UpdateStatusmanually = async (request) => {
    return PostAPI(request, 'Transaction/UpdateStatusManually');
};
export const UpdateStatusManuallyPenidngProvider = async (request) => {
    return PostAPI(request, 'Transaction/UpdateStatusManuallyPenidngProvider');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'Transaction/SelectByID');
};

export const Resend = async (request) => {
    return PostAPI(request, 'Transaction/Resend');
};

export const Shift = async (request) => {
    return PostAPI(request, 'Transaction/ShiftRecharge');
};

export const StatusCheckAll = async (request) => {
    return PostAPI(request, 'Transaction/StatusCheckAll');
};
export const DMTStatusCheckAll = async (request) => {
    return PostAPI(request, 'Transaction/GetDMTTransctionStatusCheckAll');
};

export const RefundAll = async (request) => {
    return PostAPI(request, 'Transaction/RefundAll');
};
export const RefundAllProviderPenidng = async (request) => {
    return PostAPI(request, 'Transaction/RefundAllProviderPenidng');
};
export const SuccessAllProviderPenidng = async (request) => {
    return PostAPI(request, 'Transaction/SuccessAllProviderPenidng');
};

export const SuccessAll = async (request) => {
    return PostAPI(request, 'Transaction/SuccessAll');
};

export const GetDMTTransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetDMTTransctionStatus');
};

export const GetBBPSTransctionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetBBPSTransctionStatus');
};

export const GetAEPSTransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetAEPSTransactionStatus');
};

export const GetCreditCardTransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetCreditCardTransactionStatus');
};

export const GetPayoutTransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetPayoutTransactionStatus');
};
export const GetVendorPayTransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetVendorPayTransactionStatus');
};
export const GetCashDespositTransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetCashDespositTransactionStatus');
};
export const GetRechargeTransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetRechargeTransctionStatus');
};

export const GetMicroATMTransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetMicroATMTransactionStatus');
};

export const GetWalleTopUpTransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetWalleTopUpTransactionStatus');
};

export const GetLeadGenerationTransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetLeadGenerationTransactionStatus');
};

export const PayoutTransaction = async (request) => {
    return PostAPI(request, 'Transaction/Payout');
};

export const PayoutTransactionV1 = async (request) => {
    return PostAPI(request, 'Transaction/PayoutV1');
};
export const VendorPay = async (request) => {
    return PostAPI(request, 'Transaction/VendorPay');
};

export const SelectTodaysCommission = async (request) => {
    return PostAPI(request, 'Transaction/SelectTodaysCommission');
};

export const ResendAll = async (request) => {
    return PostAPI(request, 'Transaction/ResendAll');
};

export const ShiftAll = async (request) => {
    return PostAPI(request, 'Transaction/ShiftAll');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'Transaction/MoneyTransfer');
};

export const DMTTransactionResend = async (request) => {
    return PostAPI(request, 'Transaction/DMTTransactionResend');
};

export const DMTTransactionRefund = async (request) => {
    return PostAPI(request, 'Transaction/DMTTransactionRefund');
};

export const DMTTransactionResendAll = async (request) => {
    return PostAPI(request, 'Transaction/DMTTransactionResendAll');
};

export const SelectTransferModeByIFSC = async (request) => {
    return PostAPI(request, 'Transaction/SelectTransferModeByIFSC');
};

export const VerifyOTPRefund = async (request) => {
    return PostAPI(request, 'Transaction/VerifyOTPRefund');
};

export const VerifyOTPRefundForCreditCard = async (request) => {
    return PostAPI(request, 'Transaction/VerifyOTPRefundForCreditCard');
};

export const SelectOperatorByConsumerNumber = async (request) => {
    return PostAPI(request, 'Transaction/SelectOperatorByConsumerNumber');
};



export const CMSRefundAll = async (request) => {
    return PostAPI(request, 'Transaction/CMSRefundAll');
};

export const GetTransactionRefund = async (request) => {
    return PostAPI(request, 'Transaction/GetTransactionRefund');
};

export const GetUPITransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetUPITransactionStatus');
};

export const UPITransactionResendAll = async (request) => {
    return PostAPI(request, 'Transaction/UPITransactionResendAll');
};
export const UPIStatusCheckAll = async (request) => {
    return PostAPI(request, 'Transaction/UPIStatusCheckAll');
};

export const GetOTTTransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetOTTTransactionStatus');
};

export const GetBusTransactionStatus = async (request) => {
    return PostAPI(request, 'Transaction/GetBusTransactionStatus');
};

export const GetHourlyPerformance = async (request) => {
    return PostAPI(request, 'Transaction/HourlyPerformance');
};
export const GetProviderWisePerformance = async (request) => {
    return PostAPI(request, 'Transaction/ProviderWisePerformance');
};



export const GetLast12hrPerformance = async (request) => {
    return PostAPI(request, 'Transaction/Last12hrPerformance');
};

export const PanStatusCheck = async (request) => {
    return PostAPI(request, 'PanMitra/PanStatusCheck');
};

export const TransactionStatusCheck = async (request) => {
    return PostAPI(request, 'PanMitra/TransactionStatusCheck');
}

export const AllProviderWisePerformance = async (request) => {
    return PostAPI(request, 'Transaction/AllProviderWisePerformance');
};
