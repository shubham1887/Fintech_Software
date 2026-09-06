import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'OperatorSequence/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'OperatorSequence/Select');
};

export const SelectByOperatorID = async (request) => {
    return PostAPI(request, 'OperatorSequence/SelectByOperatorID');
};

export const BulkUpdate = async (request) => {
    return PostAPI(request, 'OperatorSequence/BulkUpdate');
};
