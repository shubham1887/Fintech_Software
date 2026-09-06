import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'VirtualBalance/Add');
};

export const AddWithOTP = async (request) => {
    return PostAPI(request, 'VirtualBalance/AddWithOTP');
};

export const SelectLast10 = async (request) => {
    return PostAPI(request, 'VirtualBalance/SelectLast10');
};
export const SelectForFilter = async (request) => {
    return PostAPI(request, 'VirtualBalance/SelectForFilter');
};
