import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'ProviderOperatorDenominationMapping/Insert');
};
export const Update = async (request) => {
    return PostAPI(request, 'ProviderOperatorDenominationMapping/Update');
};

export const Delete = async (request) => {
    return PostAPI(request, 'ProviderOperatorDenominationMapping/Delete');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'ProviderOperatorDenominationMapping/SelectByID');
};

export const Select = async (request) => {
    return PostAPI(request, 'ProviderOperatorDenominationMapping/Select');
};
export const SelectDenominationMappingByProviderIDAndOperatorID = async (
    request
) => {
    return PostAPI(
        request,
        'ProviderOperatorDenominationMapping/SelectDenominationMappingByProviderIDAndOperatorID'
    );
};
export const SelectDenominationMapping = async (request) => {
    return PostAPI(
        request,
        'ProviderOperatorDenominationMapping/SelectDenominationMapping'
    );
};
