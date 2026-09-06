import { PostAPI } from 'Components/Helper/Helpers';

export const SelectBankList = async (request) => {
    return PostAPI(request, 'IMPSGuruDMT/SelectBank');
};

export const Select = async (request) => {
    return PostAPI(request, 'IMPSGuruDMT/Select');
};

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'IMPSGuruDMT/SenderRegistrationSubmitOTP');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'IMPSGuruDMT/DeleteBeneficiary');
};


export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'IMPSGuruDMT/MoneyTransfer');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'IMPSGuruDMT/AddBeneficiary');
};
