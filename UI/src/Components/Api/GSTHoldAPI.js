import { PostAPI } from 'Components/Helper/Helpers';

export const SelectGSTHold = async (request) => {
    return PostAPI(request, 'GSTHold/SelectGSTHold');
};

export const SelectForGSTHold = async (request) => {
    return PostAPI(request, 'GSTHold/SelectForGSTHold');
};

export const UpdateStatus = async (request) => {
    return PostAPI(request, 'GSTHold/UpdateStatus');
};

export const BulkUpdateGST = async (request) => {
    return PostAPI(request, 'GSTHold/BulkUpdateGST');
};
