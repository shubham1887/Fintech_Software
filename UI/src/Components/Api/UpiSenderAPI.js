import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'UpiSender/Add');
};

export const VerifyOTP = async (request) => {
    return PostAPI(request, 'UpiSender/VerifyOTP');
};

export const Select = async (request) => {
    return PostAPI(request, 'UpiSender/Select');
};
