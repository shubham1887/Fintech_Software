import { PostAPI } from 'Components/Helper/Helpers';

export const SelectByProviderID = async (request) => {
    return PostAPI(request, 'ProviderResponseMaster/Select');
};

export const Delete = async (request) => {
    return PostAPI(request, 'ProviderResponseMaster/Delete');
};

export const Activate = async (request) => {
    return PostAPI(request, 'ProviderResponseMaster/Activate');
};

export const Update = async (request) => {
    return PostAPI(request, 'ProviderResponseMaster/Update');
};

export const TestResponse = async (request) => {
    return PostAPI(request, 'ProviderResponseMaster/TestResponse');
};

export const InsertAs = async (request) => {
    return PostAPI(request, 'ProviderResponseMaster/InsertAs');
};
export const SelectBySampleID = async (request) => {
    return PostAPI(request, 'ProviderResponseMasterSample/SelectByID');
};

export const SelectForDropDown = async (request) => {
    return PostAPI(
        request,
        'ProviderResponseMasterSample/SelectDeveloperForDropDown'
    );
};
export const UpdateSampleResponseToProvider = async (request) => {
    return PostAPI(
        request,
        'ProviderResponseMasterSample/UpdateSampleResponseToProvider'
    );
};

export const SelectSampleResponseByDeveloper = async (request) => {
    return PostAPI(
        request,
        'ProviderResponseMasterSample/SelectSampleResponseByDeveloper'
    );
};

export const FetchAllSampleResponseFromSupport = async (request) => {
    return PostAPI(
        request,
        'ProviderResponseMasterSample/FetchAllSampleResponseFromSupport'
    );
};
