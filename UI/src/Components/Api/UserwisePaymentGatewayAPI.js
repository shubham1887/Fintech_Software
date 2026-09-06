import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'UserWisePayementGatewaySetting/Select');
};
export const Insert = async (request) => {
    return PostAPI(request, 'UserWisePayementGatewaySetting/Insert');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'UserWisePayementGatewaySetting/SelectByID');
};
export const Delete = async (request) => {
    return PostAPI(request, 'UserWisePayementGatewaySetting/Delete');
};
export const Update = async (request) => {
    return PostAPI(request, 'UserWisePayementGatewaySetting/Update');
};
