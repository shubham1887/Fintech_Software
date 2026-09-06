import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'ProviderPurchase/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'ProviderPurchase/Select');
};

export const Update = async (request) => {
    return PostAPI(request, 'ProviderPurchase/Update');
};

export const Delete = async (request) => {
    return PostAPI(request, 'ProviderPurchase/Delete');
};
export const SelectProviderSummary = async (request) => {
    return PostAPI(request, 'ProviderPurchase/SelectForDashboard');
};
