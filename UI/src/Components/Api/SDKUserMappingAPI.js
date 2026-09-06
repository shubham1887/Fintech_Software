import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'SDKBBPSUserMapping/Insert');
};

export const Delete = async (request) => {
    return PostAPI(request, 'SDKBBPSUserMapping/Delete');
};

export const Select = async (request) => {
    return PostAPI(request, 'SDKBBPSUserMapping/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'SDKBBPSUserMapping/SelectByID');
};
