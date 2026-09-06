import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'SupportUserAccessDetail/Select');
};

export const Activate = async (request) => {
    return PostAPI(request, 'SupportUserAccessMaster/Insert');
};
export const SelectMenu = async (request) => {
    return PostAPI(request, 'SupportUserAccessMaster/Select');
};
