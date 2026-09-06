import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'LowBalanceConfiguration/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'LowBalanceConfiguration/Select');
};
export const Delete = async (request) => {
    return PostAPI(request, 'LowBalanceConfiguration/Delete');
};
