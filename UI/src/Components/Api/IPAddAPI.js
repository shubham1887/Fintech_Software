import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'IPMaster/Insert');
};

export const Delete = async (request) => {
    return PostAPI(request, 'IPMaster/Delete');
};

export const Select = async (request) => {
    return PostAPI(request, 'IPMaster/Select');
};

export const SelectPendingIP = async (request) => {
    return PostAPI(request, 'IPMaster/SelectProcessIp');
};
export const UpdateStatus = async (request) => {
    return PostAPI(request, 'IPMaster/UpdateIpStatus');
};
