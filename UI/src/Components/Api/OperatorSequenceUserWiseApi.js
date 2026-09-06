import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'OperatorSequenceUserWise/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'OperatorSequenceUserWise/Select');
};

export const SelectByOperatorID = async (request) => {
    return PostAPI(request, 'OperatorSequenceUserWise/SelectByOperatorID');
};

export const DeleteByUserID = async (request) => {
    return PostAPI(request, 'OperatorSequenceUserWise/DeleteByUserID');
};
export const DeleteAll = async (request) => {
    return PostAPI(request, 'OperatorSequenceUserWise/DeleteAll');
};
export const SelectAllUserwiseSequenceList = async (request) => {
    return PostAPI(
        request,
        'OperatorSequenceUserWise/SelectAllUserwiseSequenceList'
    );
};
