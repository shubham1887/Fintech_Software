import { PostAPI } from 'Components/Helper/Helpers';

export const Init = async (request) => {
    return PostAPI(request, 'PaymentGateway/Initiate');
};

export const SelectByOrderID = async (request) => {
    return PostAPI(request, 'PaymentGateway/SelectByOrderID');
};

export const Select = async (request) => {
    return PostAPI(request, 'PaymentGateway/Select');
};

export const TransactionStatus = async (request) => {
    return PostAPI(request, 'PaymentGateway/TransactionStatus');
};

export const ActivePaymentGateway = async (request) => {
    return PostAPI(request, 'PaymentGateway/SelectActivePaymentGateway');
};

export const SelectStaticQR = async (request) => {
    return PostAPI(request, 'PaymentGateway/SelectStaticQR');
};
export const ActivePaymentGatewayV2 = async (request) => {
    return PostAPI(request, 'PaymentGateway/SelectActivePaymentGatewayV2');
};

export const UpdateStatusmanually = async (request) => {
    return PostAPI(request, 'PaymentGateway/UpdateStatusManually');
};

export const SelectByID = async (request) => {
    return PostAPI(request, 'PaymentGateway/SelectByID');
};
export const SelectRoleCommission = async (request) => {
    return PostAPI(request, 'PaymentGateway/SelectRoleWithCommission');
};
export const SelectLast10PaymentGateway = async (request) => {
    return PostAPI(request, 'PaymentGateway/SelectLast10PaymentGateway');
};
export const SelectLast10CreditToBank = async (request) => {
    return PostAPI(request, 'PaymentGateway/SelectLast10CreditToBank');
};
