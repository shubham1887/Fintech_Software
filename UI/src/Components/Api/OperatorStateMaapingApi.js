import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'OperatorStateMapping/Select');
};

export const Add = async (request) => {
    return PostAPI(request, 'OperatorStateMapping/Insert');
};
export const Activate = async (request) => {
    return PostAPI(request, 'OperatorStateMapping/ActiveDeactive');
};
export const Delete = async (request) => {
    return PostAPI(request, 'OperatorStateMapping/Delete');
};
export const SelectByID = async (request) => {
    return PostAPI(
        request,
        'OperatorStateMapping/SelectByOperatorStateMappingID'
    );
};
export const Update = async (request) => {
    return PostAPI(request, 'OperatorStateMapping/Update');
};
