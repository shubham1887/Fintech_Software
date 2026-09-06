import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'LapuMaster/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'LapuMaster/Select');
};

export const Delete = async (request) => {
    return PostAPI(request, 'LapuMaster/Delete');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'LapuMaster/SelectByID');
};
export const Update = async (request) => {
    return PostAPI(request, 'LapuMaster/Update');
};
