import { PostAPI } from 'Components/Helper/Helpers';

export const Upload = async (request) => {
    return PostAPI(request, 'Reconciliation/Upload');
};
