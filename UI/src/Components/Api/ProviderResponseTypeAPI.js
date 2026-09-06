import { PostAPI } from 'Components/Helper/Helpers';

export const SelectForDropDown = async (request) => {
    return PostAPI(request, 'ProviderResponseTypeMaster/Select');
};
