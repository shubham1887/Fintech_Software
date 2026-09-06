import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'Slabs/Insert');
};

export const Copy = async (request) => {
    return PostAPI(request, 'Slabs/Copy');
};
export const Select = async (request) => {
    return PostAPI(request, 'Slabs/Select');
};

export const SelectSlab = async (request) => {
    return PostAPI(request, 'Slabs/SelectSlabForExcel');
};

export const Activate = async (request) => {
    return PostAPI(request, 'Slabs/ActiveDeactive');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'Slabs/SelectByID');
};
export const Update = async (request) => {
    return PostAPI(request, 'Slabs/Update');
};
export const Delete = async (request) => {
    return PostAPI(request, 'Slabs/Delete');
};
