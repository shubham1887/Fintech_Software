export const SERVER_DOWN_ERROR =
    'Error : Server down! Please try again after some time!';

export const UNABLE_TO_PROCESS_REQUEST_ERROR =
    'Unable to process request. Please try after some time!';
export const PLEASE_PROVIDE_VALID = 'Please provide valid ';
export const PLEASE_SELECT_VALID = 'Please select valid ';
export const IS_CASH_COLLECTION = 'true';
export const CASH = 'Cash';

export const MESSAGE_CLOSE_AFTER = 2 * 1000;

export const IMPS = "IMPS";
export const ADMIN_CODE = 1;
export const SUPERDIST_CODE = 2;
export const DIST_CODE = 3;
export const RETAILER_CODE = 4;
export const API_USER_CODE = 5;
export const SUPPORT_CODE = 6;
export const CLIENT_CODE = 7;

export const DEVELOPER_CODE = 8;

export const MAIN_WalletID = 1;
export const DMT_WalletID = 2;
export const AEPS_WalletID = 6;
export const AEPS_CODE = 4;
export const CREDITCARDPAYMENT_CODE = 16;
export const CASHDEPOSIT_CODE = 17;
export const WALLETTOPUP_CODE = 18;
export const CREDITCARDPAYMENT_OPERATOR_CODE = 27;
export const MONEY_TRANSFER_OPERATOR_TRANSFER_CODE = 13;
export const MONEY_TRANSFER_OPERATOR_VERIFY_CODE = 12;

export const FUND_TRANSFER = 1;
export const CABLE_TV_CODE = 7;
export const DTH_BOOKING = 8;
export const PHONE_BOOKING = 9;

export const PAYMENT_GATEWAY_CODE = 3;
export const MONEY_TRANSFER_CODE = 10;
export const PREPAID_RECHARGE_CODE = 11;
export const ELECTRICITY_CODE = 12;
export const DTH_RECHARGE_CODE = 13;
export const POSTPAID_RECHARGE_CODE = 14;
export const PANCARD_CODE = 34;
export const GENERAL_INSURANCE_CODE = 19;
export const FRC_RECHARGE_CODE = 20;
export const INSURANCE_CODE = 21;
export const FAST_TAG_CODE = 22;
export const LANDLINE_CODE = 23;
export const BROADBAND_CODE = 24;
export const WATER_BILL_CODE = 26;
export const PIPED_GAS = 27;
export const Cylinder_GAS = 25;
export const PAYOUT = 28;
export const LOAN_PAYMENT_CODE = 29;
export const UPITRANSFER_CODE = 31;
export const ACCOUNT_OPENING_CODE = 30;
export const GAME_CODE = 2;
export const CMS_CODE = 36;
export const LAPU_LOADING_CODE = 15;
export const MICRO_ATM_CODE = 5;
export const IMPS_OPERATOR_ID = 56;
export const UPI_OPERATOR_ID = 57;
export const PREPAID_ACTIVATION_CODE = 6;
export const ADMIN_USERID = 2;
export const AEPS_Onboarding = 32;
export const MATM_Onboarding = 33;
export const DEMAT = 37;
export const E_KYC = -99;
export const VENDOR_PAY = 39;
export const OTT = 40;
export const LEAD_GENERATION = 41;
export const BUS_BOOKING = 42;
export const CREDITCARD_TO_BANK = 43;
export const FEE_PAYMENT = 44;
export const MUNCIPAL_TAXES = 45;
export const TRAFFIC_CHLLAN = 46;
export const HOSPITAL = 47;
export const AEPS_OPERATOR_ID = 5;
export const AADHARPAY_OPERATOR_ID = 11;
export const HOUSING_SOCIETY = 53;
export const CONTRA_VOUCHER_ID = 4;
export const PAYMENT_VOUCHER_ID = 3;
export const RECEIPT_VOUCHER_ID = 1;
export const HOSPITAL_PATHOLOGY = 53;
export const EZYPAY_SERVICECODE = 51;
export const AFFILIATE_INSURANCE = 49;

export const FIRST_DATE_OF_MONTH = () => {
    let date = new Date();
    return new Date(date.getFullYear(), date.getMonth(), 1);
};

export const GET_PRIMARY_ID_FROM_URL = () => {
    let URL = window.location.pathname;
    var res = URL.split('/');
    let id = res[res.length - 1];
    return id;
};

export const GET_PREVIOUS_MONTH = new Date().getMonth() + 1 - 1;

export const GET_YEAR_FOR_GST = () => {
    let year = new Date().getFullYear();
    const current = new Date();
    current.setMonth(current.getMonth() - 1);
    const previousMonth = current.toLocaleString('default', { month: 'long' });

    if (previousMonth === 'December') year = year - 1;

    return year;
};

export const GET_Month_FOR_GST = () => {
    // const current = new Date();
    // current.setMonth(current.getMonth() - 1);
    //   const previousMonth = current.toLocaleString('default', { month: 'long' });
    // return previousMonth;
    let month = new Date().getMonth() + 1 - 1;
    if (month === 0) month = 12;
    return month;
};

export const GET_CURRENT_YEAR = new Date().getFullYear();

export const REFRESH_BALANCE_FLAG = 'Refresh Balance';
