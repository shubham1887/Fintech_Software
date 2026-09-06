import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'MenuMaster/SelectByUsertypeID');
};
export const SelectMenu = async (request) => {
    return PostAPI(request, 'MenuMaster/SelectMenu');
};
export const SelectParentMenuDropDown = async (request) => {
    return PostAPI(request, 'MenuMaster/SelectParentMenu');
};

export const SelectAll = async (request) => {
    return PostAPI(request, 'MenuMaster/Select');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'MenuMaster/SelectByID');
};
export const Insert = async (request) => {
    return PostAPI(request, 'MenuMaster/Insert');
};

export const Update = async (request) => {
    return PostAPI(request, 'MenuMaster/Update');
};
export const Delete = async (request) => {
    return PostAPI(request, 'MenuMaster/Delete');
};
