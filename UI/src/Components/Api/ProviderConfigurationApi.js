import { PostAPI } from 'Components/Helper/Helpers';

export const SelectByProviderID = async (request) => {
    return PostAPI(request, 'ProviderConfiguration/SelectByProviderID');
};

export const SelectByProviderIDOperatorID = async (request) => {
    return PostAPI(
        request,
        'ProviderConfiguration/SelectByProviderIDOperatorID'
    );
};

export const Activate = async (request) => {
    return PostAPI(request, 'ProviderConfiguration/ActiveDeactive');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'ProviderConfiguration/SelectByID');
};

export const Update = async (request) => {
    return PostAPI(request, 'ProviderConfiguration/Update');
};

export const CirecleMappingInsert = async (request) => {
    return PostAPI(request, 'ProviderConfiguration/CirecleMappingInsert');
};

export const UpdateAll = async (request) => {
    return PostAPI(request, 'ProviderConfiguration/UpdateAll');
};

export const SelectSampleProvider = async (request) => {
    return PostAPI(request, 'ProviderConfigurationSample/Select');
};

export const SelectBySampleID = async (request) => {
    return PostAPI(request, 'ProviderConfigurationSample/SelectByID');
};
export const SelectDeveloperForDropDown = async (request) => {
    return PostAPI(
        request,
        'ProviderConfigurationSample/SelectDeveloperForDropDown'
    );
};

export const UpdateSampleConfiguration = async (request) => {
    return PostAPI(
        request,
        'ProviderConfigurationSample/UpdateSampleConfiguration'
    );
};

export const SelectSampleConfigurationByDeveloper = async (request) => {
    return PostAPI(
        request,
        'ProviderConfigurationSample/SelectSampleConfigurationByDeveloper'
    );
};
export const FetchAllSampleConfigurationFromSupport = async (request) => {
    return PostAPI(
        request,
        'ProviderConfigurationSample/FetchAllSampleConfigurationFromSupport'
    );
};
