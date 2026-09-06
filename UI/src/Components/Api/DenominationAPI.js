import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'DenominationMaster/Insert');
};
export const Select = async (request) => {
    return PostAPI(request, 'DenominationMaster/Select');
};
export const Delete = async (request) => {
    return PostAPI(request, 'DenominationMaster/Delete');
};
export const Update = async (request) => {
    return PostAPI(request, 'DenominationMaster/Update');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'DenominationMaster/SelectByID');
};

export const SelectForDropDown = async (request) => {
    return PostAPI(request, 'DenominationMaster/SelectForDropDown');
};
