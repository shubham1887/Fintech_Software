import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'RangeWiseSequence/Select');
};
export const Insert = async (request) => {
    return PostAPI(request, 'RangeWiseSequence/Insert');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'RangeWiseSequence/SelectByID');
};
export const Delete = async (request) => {
    return PostAPI(request, 'RangeWiseSequence/Delete');
};
export const Update = async (request) => {
    return PostAPI(request, 'RangeWiseSequence/Update');
};
