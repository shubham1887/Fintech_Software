import { PostAPI } from 'Components/Helper/Helpers';

export const BillFetch = async (request) => {
    return PostAPI(request, 'Utility/BillFetch');
};
