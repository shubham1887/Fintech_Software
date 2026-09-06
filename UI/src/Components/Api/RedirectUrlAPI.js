import { PostAPI } from 'Components/Helper/Helpers';

export const SelectRedirectUrlByName = async (request) => {
    return PostAPI(request, 'RedirectUrlMaster/SelectRedirectUrlByName');
};
