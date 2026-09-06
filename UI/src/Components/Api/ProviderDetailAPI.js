import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'ProviderDetails/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'ProviderDetails/Select');
};
export const Activate = async (request) => {
    return PostAPI(request, 'ProviderDetails/ActiveDeactive');
};
export const Delete = async (request) => {
    return PostAPI(request, 'ProviderDetails/Delete');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'ProviderDetails/SelectByProviderDetailID');
};
export const Update = async (request) => {
    return PostAPI(request, 'ProviderDetails/Update');
};
export const SelectByProviderID = async (request) => {
    return PostAPI(request, 'ProviderDetails/SelectByProviderID');
};

export const SelectSampleResponse = async (request) => {
    return PostAPI(request, 'ProviderResponseMasterSample/Select');
};
export const FetchAllSampleURLFromSupport = async (request) => {
    return PostAPI(
        request,
        'ProviderDetailsSample/FetchAllSampleURLFromSupport'
    );
};
