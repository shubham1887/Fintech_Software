import { PostAPI } from 'Components/Helper/Helpers';

export const Search = async (request) => {
    return PostAPI(
        request,
        'ProviderRequestResponseLog/SelectByConsumerNumber'
    );
};
export const SearchByID = async (request) => {
    return PostAPI(
        request,
        'ProviderRequestResponseLog/SelectByTansactionMasterID'
    );
};
export const SelectByUserID = async (request) => {
    return PostAPI(request, 'ProviderRequestResponseLog/SelectByUserID');
};

export const SelectByDate = async (request) => {
    return PostAPI(
        request,
        'ProviderRequestResponseLog/SelectByFromDateTodate'
    );
};

export const SelectByDatev1 = async (request) => {
    return PostAPI(request, 'ProviderCallBackLog/SelectByFromDateTodate');
};
