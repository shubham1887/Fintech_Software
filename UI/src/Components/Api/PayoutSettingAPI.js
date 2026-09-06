import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'PayoutSetting/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'PayoutSetting/Select');
};

export const IsOnOff = async (request) => {
    return PostAPI(request, 'PayoutSetting/IsOnOff');
};

export const SelectActivePayout = async (request) => {
    return PostAPI(request, 'PayoutSetting/SelectActivePayout');
};
