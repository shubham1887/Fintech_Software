import { PostAPI } from 'Components/Helper/Helpers';

export const SelectOnlyRetailerAndAPI = async (request) => {
    return PostAPI(request, 'UserType/SelectOnlyRetailerAndAPI');
};

export const Insert = async (request) => {
    return PostAPI(request, 'UserWisePlanLimitSetting/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'UserWisePlanLimitSetting/Select');
};

export const Update = async (request) => {
    return PostAPI(request, 'UserWisePlanLimitSetting/Update');
};

export const Delete = async (request) => {
    return PostAPI(request, 'UserWisePlanLimitSetting/Delete');
};

export const ActiveDeactive = async (request) => {
    return PostAPI(request, 'UserWisePlanLimitSetting/ActiveDeactive');
};

export const AllActiveDeactive = async (request) => {
    return PostAPI(request, 'UserWisePlanLimitSetting/AllActiveDeactive');
};

export const SelectReport = async (request) => {
    return PostAPI(request, 'UserWisePlanView/Select');
};