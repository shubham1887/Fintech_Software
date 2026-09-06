import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'AccountOpenSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'AccountOpenSetting/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'AccountOpenSetting/SelectByID');
};

export const SelectActive = async (request) => {
    return PostAPI(request, 'AccountOpenSetting/SelectActive');
};
