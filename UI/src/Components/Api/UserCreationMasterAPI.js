import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'UserCreationMaster/Insert');
};
export const Select = async (request) => {
    return PostAPI(request, 'UserCreationMaster/Last10Report');
};
export const FreeTransfer = async (request) => {
    return PostAPI(request, 'UserCreationMaster/FreeTransfer');
};

export const SelectUserFilter = async (request) => {
    return PostAPI(request, 'UserCreationMaster/SelectForFilter');
};
