import { PostAPI } from 'Components/Helper/Helpers';

export const SendBulkMessage = async (request) => {
    return PostAPI(request, 'BulkMessage/Insert');
};

export const SelectBulkMessage = async (request) => {
    return PostAPI(request, 'BulkMessage/Select');
};

export const DeleteAll = async (request) => {
    return PostAPI(request, 'BulkMessage/DeleteAll');
};
