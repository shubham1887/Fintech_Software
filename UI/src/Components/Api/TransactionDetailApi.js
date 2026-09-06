import { PostAPI } from 'Components/Helper/Helpers';

export const SelectForFilter = async (request) => {
    return PostAPI(request, 'TransactionDetails/SelectForFilter');
};
export const SelectAdminAccounting = async (request) => {
    return PostAPI(request, 'TransactionDetails/SelectAdminAccounting');
};
export const SelectMarketAccounting = async (request) => {
    return PostAPI(request, 'TransactionDetails/SelectMarketAccounting');
};
