import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'ProviderConfigurationAdminCommission/Insert');
};
export const Update = async (request) => {
    return PostAPI(request, 'ProviderConfigurationAdminCommission/Update');
};
export const Delete = async (request) => {
    return PostAPI(request, 'ProviderConfigurationAdminCommission/Delete');
};
export const SelectByServiceID = async (request) => {
    return PostAPI(
        request,
        'ProviderConfigurationAdminCommission/SelectByServiceID'
    );
};
