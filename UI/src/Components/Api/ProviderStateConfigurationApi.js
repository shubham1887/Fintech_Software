import { PostAPI } from 'Components/Helper/Helpers';

export const SelectByProviderID = async (request) => {
    return PostAPI(request, 'ProviderStateConfiguration/SelectByProviderID');
};

export const Update = async (request) => {
    return PostAPI(request, 'ProviderStateConfiguration/Update');
};

export const UpdateAll = async (request) => {
    return PostAPI(request, 'ProviderStateConfiguration/UpdateAll');
};
