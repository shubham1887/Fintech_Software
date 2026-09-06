import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'UserwiseDMTSequence/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'UserwiseDMTSequence/Select');
};

export const DeleteByUserID = async (request) => {
    return PostAPI(request, 'UserwiseDMTSequence/DeleteByUserID');
};

export const DeleteAll = async (request) => {
    return PostAPI(request, 'UserwiseDMTSequence/DeleteAll');
};

