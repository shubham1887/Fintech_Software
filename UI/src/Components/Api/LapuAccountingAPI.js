import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'LapuPartyMaster/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'LapuPartyMaster/Select');
};

export const Delete = async (request) => {
    return PostAPI(request, 'LapuPartyMaster/Delete');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'LapuPartyMaster/SelectByID');
};
export const Update = async (request) => {
    return PostAPI(request, 'LapuPartyMaster/Update');
};

export const Purchase = async (request) => {
    return PostAPI(request, 'LapuTransactionDetail/Insert');
};

export const LapuPurchaseSelect = async (request) => {
    return PostAPI(request, 'LapuTransactionDetail/Select');
};
export const LapuPurchaseAmountUpdate = async (request) => {
    return PostAPI(request, 'LapuTransactionDetail/Update');
};
export const SelectUsageReport = async (request) => {
    return PostAPI(request, 'LapuTransactionDetail/SelectUsageReport');
};
