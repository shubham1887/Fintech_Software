import { PostAPI } from 'Components/Helper/Helpers';

export const Insert = async (request) => {
    return PostAPI(request, 'Complaints/Add');
};

export const Update = async (request) => {
    return PostAPI(request, 'Complaints/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'Complaints/Select');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'Complaints/SelectByID');
};

export const SelectForComplaint = async (request) => {
    return PostAPI(request, 'Complaints/SelectForComplaint');
};
export const SelectDashboradComplaint = async (request) => {
    return PostAPI(request, 'Complaints/SelectDashboradComplaint');
};

export const AcceptAll = async (request) => {
    return PostAPI(request, 'Complaints/AcceptAll');
};

export const RejectAll = async (request) => {
    return PostAPI(request, 'Complaints/RejectAll');
};
