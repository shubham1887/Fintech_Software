import { PostAPI } from 'Components/Helper/Helpers';

export const SelectAll = async (request) => {
    return PostAPI(request, 'Service/SelectAll');
};
export const SelectChargeServices = async (request) => {
    return PostAPI(request, 'Service/SelectChargeServices');
};
export const Activate = async (request) => {
    return PostAPI(request, 'Service/ActiveDeactive');
};
export const AllSericesActiveDeactiveVeriftyOTP = async (request) => {
    return PostAPI(request, 'Service/AllSericesActiveDeactiveVeriftyOTP');
};
export const AllSericesActiveDeactiveGenerateOTP = async (request) => {
    return PostAPI(request, 'Service/AllSericesActiveDeactiveGenerateOTP');
};

export const SelectByRechargeServices = async (request) => {
    return PostAPI(request, 'Service/SelectByRechargeServices');
};

export const SelectByRechargeDTH = async (request) => {
    return PostAPI(request, 'Service/SelectByRechargeDTH');
};
export const SelectServiceMoneyTransferAndUPITransfer = async (request) => {
    return PostAPI(request, 'Service/SelectServiceMoneyTransferAndUPITransfer');
};

export const Update = async (request) => {
    return PostAPI(request, 'Service/Update');
};

export const Add = async (request) => {
    return PostAPI(request, 'Service/Insert');
};

export const ServiceSelectByID = async (request) => {
    return PostAPI(request, 'Service/SelectByServiceID');
};
export const SelectByServiceType = async (request) => {
    return PostAPI(request, 'Service/SelectByServicesType');
};
export const SelectByOfflineServices = async (request) => {
    return PostAPI(request, 'Service/SelectByOfflineServices');
};
export const SelectByPrimeServices = async (request) => {
    return PostAPI(request, 'Service/SelectByPrimeServices');
};
export const SelectPrePaidDTHFRCService = async (request) => {
    return PostAPI(request, 'Service/SelectPrePaidDTHFRCService');
};
export const SelectForCallBackUrl = async (request) => {
    return PostAPI(request, 'Service/SelectForCallBackUrl');
};

export const Display = async (request) => {
    return PostAPI(request, 'Service/DisplayOnOff');
};

export const DisplayDefault = async (request) => {
    return PostAPI(request, 'Service/DefaultIsOnOff');
};

export const DefaultOutside = async (request) => {
    return PostAPI(request, 'Service/OutsideIsOnOff');
};

export const SubscriptionAllowOnOFF = async (request) => {
    return PostAPI(request, 'Service/SubscriptionAllowOnOFF');
};

export const SelectSubscriptionAllowedServices = async (request) => {
    return PostAPI(request, 'Service/SelectSubscriptionAllowedServices');
};
export const SelectRangeWiseServices = async (request) => {
    return PostAPI(request, 'Service/SelectRangeWiseServices');
};

export const SelectBBPSEnabledServices = async (request) => {
    return PostAPI(request, 'Service/SelectBBPSEnabledServices');
};

export const SelectCMS = async (request) => {
    return PostAPI(request, 'CMSSetting/SelectActiveCMS');
};

export const SelectSubscriptionServices = async (request) => {
    return PostAPI(request, 'Service/SelectSubscriptionServices');
};
