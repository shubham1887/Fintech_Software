import { PostAPI } from 'Components/Helper/Helpers';

export const SelectIncome = async (request) => {
    return PostAPI(request, 'TransactionReport/Income');
};
