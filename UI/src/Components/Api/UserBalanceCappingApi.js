import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'UserBalanceCapping/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'UserBalanceCapping/Select');
};
export const Delete = async (request) => {
    return PostAPI(request, 'UserBalanceCapping/Delete');
};
export const Update = async (request) => {
    return PostAPI(request, 'UserBalanceCapping/Update');
};
