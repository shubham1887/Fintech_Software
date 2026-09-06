import { PostAPI } from '../Helper/Helpers';

export const SelectFilter = async (request) => {
    return PostAPI(request, 'PrepaidActivation/SelectFilter');
};
export const ActivationInsert = async (request) => {
    return PostAPI(request, 'PrepaidActivation/Insert');
};

export const SelectActivation = async (request) => {
    return PostAPI(request, 'PrepaidActivation/Select');
};
export const Update = async (request) => {
    return PostAPI(request, 'PrepaidActivation/Update');
};
