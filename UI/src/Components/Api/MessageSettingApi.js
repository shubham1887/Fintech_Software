import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'MessageSetting/Select');
};

export const IsOTPactivate = async (request) => {
    return PostAPI(request, 'MessageSetting/IsOTPActiveDeactive');
};

export const IsSMSactivate = async (request) => {
    return PostAPI(request, 'MessageSetting/IsSMSActiveDeactive');
};

export const IsMailactivate = async (request) => {
    return PostAPI(request, 'MessageSetting/IsMailActiveDeactive');
};

export const IsWhatsUpActiveDeactive = async (request) => {
    return PostAPI(request, 'MessageSetting/IsWhatsUpActiveDeactive');
};
export const Update = async (request) => {
    return PostAPI(request, 'MessageSetting/Update');
};

export const SelectForFilter = async (request) => {
    return PostAPI(request, 'MessageSetting/SelectForFilter');
};
