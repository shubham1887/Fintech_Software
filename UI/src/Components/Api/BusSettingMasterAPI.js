import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'BusSettingMaster/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'BusSettingMaster/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'BusSettingMaster/SelectByID');
};

export const SelectActive = async (request) => {
    return PostAPI(request, 'BusSettingMaster/SelectActiveBusSetting');
};

export const SelectActiveBusSetting = async (request) => {
    return PostAPI(request, 'BusSettingMaster/SelectActive');
};
