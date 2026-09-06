import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'Invoice/Select');
};

export const SelectByType = async (request) => {
    return PostAPI(request, 'Invoice/SelectByType');
};
