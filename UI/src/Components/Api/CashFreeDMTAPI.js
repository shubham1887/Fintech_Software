import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'CashFreeDMT/AddBeneficiary');
};

export const DMTMoneyTransfer = async (request) => {
    return PostAPI(request, 'CashFreeDMT/DMTMoneyTransfer');
};
export const SenderSelect = async (request) => {
    return PostAPI(request, 'CashFreeDMT/SenderSelect');
};

export const BeneficiarySelect = async (request) => {
    return PostAPI(request, 'CashFreeDMT/Select');
};

export const BeneficiaryDelete = async (request) => {
    return PostAPI(request, 'CashFreeDMT/Delete');
};
