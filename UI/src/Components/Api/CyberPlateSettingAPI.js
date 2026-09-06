import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'CyberPlateSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'CyberPlateSetting/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'CyberPlateSetting/SelectByID');
};
