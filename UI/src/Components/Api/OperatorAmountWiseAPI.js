import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'OperatorAmountWiseSetting/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'OperatorAmountWiseSetting/Select');
};

export const Update = async (request) => {
    return PostAPI(request, 'OperatorAmountWiseSetting/Update');
};

export const Delete = async (request) => {
    return PostAPI(request, 'OperatorAmountWiseSetting/Delete');
};
