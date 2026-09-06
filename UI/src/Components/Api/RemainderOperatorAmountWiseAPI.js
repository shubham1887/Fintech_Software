import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'RemainderOperatorAmountWise/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'RemainderOperatorAmountWise/Select');
};

export const Update = async (request) => {
    return PostAPI(request, 'RemainderOperatorAmountWise/Update');
};

export const Delete = async (request) => {
    return PostAPI(request, 'RemainderOperatorAmountWise/Delete');
};
