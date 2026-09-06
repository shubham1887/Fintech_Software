import { PostAPI } from 'Components/Helper/Helpers';

export const SelectSender = async (request) => {
    return PostAPI(request, 'SenderBeneficiary/SelectSender');
};

export const SenderActiveDeactive = async (request) => {
    return PostAPI(request, 'SenderBeneficiary/SenderActiveDeactive');
};

export const SelectBeneficiary = async (request) => {
    return PostAPI(request, 'SenderBeneficiary/SelectBeneficiary');
};

export const BeneficiaryActDeact = async (request) => {
    return PostAPI(request, 'SenderBeneficiary/BeneficiaryActDeact');
};
