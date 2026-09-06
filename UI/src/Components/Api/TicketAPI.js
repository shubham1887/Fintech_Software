import { PostAPI } from 'Components/Helper/Helpers';

export const selectTicketCategory = async (request) => {
    return PostAPI(request, 'TicketMaster/SelectTicketCategory');
};

export const Insert = async (request) => {
    return PostAPI(request, 'TicketMaster/Insert');
};

export const Select = async (request) => {
    return PostAPI(request, 'TicketMaster/Select');
};
export const SelectAll = async (request) => {
    return PostAPI(request, 'TicketMaster/SelectAll');
};
export const Delete = async (request) => {
    return PostAPI(request, 'TicketMaster/Delete');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'TicketMaster/SelectByID');
};
export const Update = async (request) => {
    return PostAPI(request, 'TicketMaster/Update');
};

export const UpdateAction = async (request) => {
    return PostAPI(request, 'TicketMaster/UpdateAction');
};
