import { PostAPI } from 'Components/Helper/Helpers';

export const CreditCardPayment = async (request) => {
    return PostAPI(request, 'PayUCreditCard/PayBill');
};
