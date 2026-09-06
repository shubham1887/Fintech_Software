import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'ThirdPartyServiceProvider/Insert');
};

export const Update = async (request) => {
    return PostAPI(request, 'ThirdPartyServiceProvider/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'ThirdPartyServiceProvider/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'ThirdPartyServiceProvider/SelectByID');
};

export const SelectURL = async (request) => {
    return PostAPI(request, 'InstantPayWebSDK/SelectParameters');
};
