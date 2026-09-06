import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'BBPSConfiguration/Insert');
};
export const Select = async (request) => {
    return PostAPI(request, 'BBPSConfiguration/Select');
};

export const SelectByOperatorID = async (request) => {
    return PostAPI(request, 'BBPSConfiguration/SelectByOperatorID');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'BBPSConfiguration/SelectByID');
};
export const Update = async (request) => {
    return PostAPI(request, 'BBPSConfiguration/Update');
};
export const Delete = async (request) => {
    return PostAPI(request, 'BBPSConfiguration/Delete');
};
