import { PostAPI } from 'Components/Helper/Helpers';

export const CreateVPA = async (request) => {
    return PostAPI(request, 'PaysprintUPICollection/CreateVPAUPICollection');
};
export const GetDetails = async (request) => {
    return PostAPI(request, 'PaysprintUPICollection/GetDetailsUPICollection');
};
export const StaticQR = async (request) => {
    return PostAPI(request, 'PaysprintUPICollection/StaticQR');
};
export const DynamicQR = async (request) => {
    return PostAPI(request, 'PaysprintUPICollection/DynamicQR');
};

export const CreateRPQR = async (request) => {
    return PostAPI(request, 'RazorPayQRPG/CreateQR');
};
export const GetDetailsRazorPay = async (request) => {
    return PostAPI(request, 'RazorPayQRPG/GetDetails');
};
