import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'VirtualAccountMaster/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'VirtualAccountMaster/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'VirtualAccountMaster/SelectByID');
};

export const SelectVirtualAccount = async (request) => {
    return PostAPI(request, 'VirtualAccountMaster/SelectVirtualAccount');
};
