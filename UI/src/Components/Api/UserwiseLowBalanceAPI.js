import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'UserwiseLowBalance/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'UserwiseLowBalance/Select');
};

export const Insert = async (request) => {
    return PostAPI(request, 'UserwiseLowBalance/Insert');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'UserwiseLowBalance/SelectByID');
};

export const SelectLowBalance = async (request) => {
    return PostAPI(request, 'UserwiseLowBalance/SelectLowBalance');
};
