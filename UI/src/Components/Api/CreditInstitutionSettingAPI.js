import { PostAPI } from 'Components/Helper/Helpers';

export const Update = async (request) => {
    return PostAPI(request, 'CreditInstitution/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'CreditInstitution/Select');
};

export const IsOnOff = async (request) => {
    return PostAPI(request, 'CreditInstitution/IsOnOff');
};

export const SelectReport = async (request) => {
    return PostAPI(request, 'CreditInstitution/SelectReport');
};

export const UpdateBalance = async (request) => {
    return PostAPI(request, 'CreditInstitution/UpdateUserBalance');
};
