import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'SettingMaster/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'SettingMaster/SelectByID');
};
export const Update = async (request) => {
    return PostAPI(request, 'SettingMaster/Update');
};

export const IsSecure = async (request) => {
    return PostAPI(request, 'SettingMaster/IsSecureOnOff');
};
export const SelectValueByKey = async (request) => {
    return PostAPI(request, 'SettingMaster/SelectValueByKey');
};

export const SelectOnlineDMTValue = async (request) => {
    return PostAPI(request, 'SettingMaster/SelectOnlineDMTValue');
};

export const UpdateOnlineDMTValue = async (request) => {
    return PostAPI(request, 'SettingMaster/UpdateOnlineDMTValue');
};
