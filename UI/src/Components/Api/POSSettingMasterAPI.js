import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'POSSettingMaster/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'POSSettingMaster/Select');
};

export const IsOnOff = async (request) => {
    return PostAPI(request, 'POSSettingMaster/IsOnOff');
};
