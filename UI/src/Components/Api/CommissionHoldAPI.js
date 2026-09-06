import { PostAPI } from 'Components/Helper/Helpers';

export const SelectCommissionHold = async (request) => {
    return PostAPI(request, 'CommissionHold/SelectCommissionHold');
};
export const Select = async (request) => {
    return PostAPI(request, 'Redeemcommission/Select');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'Redeemcommission/SelectByID');
};
export const Update = async (request) => {
    return PostAPI(request, 'Redeemcommission/Update');
};
export const Insert = async (request) => {
    return PostAPI(request, 'Redeemcommission/Insert');
};
export const GenerateAll = async (request) => {
    return PostAPI(request, 'Redeemcommission/GenerateAll');
};

export const SelectAll = async (request) => {
    return PostAPI(request, 'Redeemcommission/SelectAll');
};

export const SelectReport = async (request) => {
    return PostAPI(request, 'Redeemcommission/SelectReport');
};

export const BulkCommisssionUpdate = async (request) => {
    return PostAPI(request, 'Redeemcommission/BulkCommisssionUpdate');
};

export const CommisssionHoldByUserID = async (request) => {
    return PostAPI(request, 'Redeemcommission/CommisssionHoldByUserID');
};
