import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'SMSMaster/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'SMSMaster/Select');
};

export const Activate = async (request) => {
    return PostAPI(request, 'SMSMaster/ActiveDeactive');
};

export const SelectBalance = async (request) => {
    return PostAPI(request, 'SMSMaster/CheckBalance');
};
