import { PostAPI } from 'Components/Helper/Helpers';

export const CreditCardPayment = async (request) => {
    return PostAPI(request, 'PaysprintCreditCardPayment/PayBill');
};

export const GenerateOTP = async (request) => {
    return PostAPI(request, 'PaysprintCreditCardPayment/GenerateOTP');
};

export const StatusCheck = async (request) => {
    return PostAPI(request, 'PaysprintCreditCardPayment/StatusCheck');
};

export const ResendOTP = async (request) => {
    return PostAPI(request, 'PaysprintCreditCardPayment/ResendOTP');
};
