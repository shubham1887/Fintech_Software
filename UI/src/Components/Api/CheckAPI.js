import { PostAPI } from 'Components/Helper/Helpers';

export const Testsms = async (request) => {
    return PostAPI(request, 'Check/CheckSms');
};
export const TestEmail = async (request) => {
    return PostAPI(request, 'Check/CheckEmail');
};
export const TestWhatsAppSms = async (request) => {
    return PostAPI(request, 'Check/CheckWhatsAppSms');
};
