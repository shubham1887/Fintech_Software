import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'WebHookSetting/Select');
};

export const Isactivate = async (request) => {
    return PostAPI(request, 'WebHookSetting/IsActiveDeactive');
};

export const Update = async (request) => {
    return PostAPI(request, 'WebHookSetting/Update');
};
export const SelectByDate = async (request) => {
    return PostAPI(request, 'WebHookLog/SelectByDate');
};
