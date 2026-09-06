import { PostAPI } from 'Components/Helper/Helpers';

export const Search = async (request) => {
    return PostAPI(request, 'ProviderCallBackLog/SelectByConsumerNumber');
};
export const SearchByID = async (request) => {
    return PostAPI(request, 'ProviderCallBackLog/SelectByTansactionMasterID');
};
