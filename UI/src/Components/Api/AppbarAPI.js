import { PostAPI } from 'Components/Helper/Helpers';

export const Refresh = async (request) => {
    return PostAPI(request, 'Appbar/Refresh');
};

export const RefreshSD = async (request) => {
    return PostAPI(request, 'Appbar/RefreshSD');
};
