import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'OTTSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'OTTSetting/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'OTTSetting/SelectByID');
};

export const SelectActive = async (request) => {
    return PostAPI(request, 'OTTSetting/SelectActive');
};
