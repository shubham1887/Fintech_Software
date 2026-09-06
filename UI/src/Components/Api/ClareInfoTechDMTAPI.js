import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'ClareInfoTechDMT/Select');
};

export const SelectBene = async (request) => {
    return PostAPI(request, 'ClareInfoTechDMT/Select');
};

export const AddBeneficiary = async (request) => {
    return PostAPI(request, 'ClareInfoTechDMT/AddBeneficiary');
};

export const MoneyTransfer = async (request) => {
    return PostAPI(request, 'ClareInfoTechDMT/MoneyTransfer');
};

export const SelectBank = async (request) => {
    return PostAPI(request, 'ClareInfoTechDMT/SelectBank');
};

export const SenderRegistration = async (request) => {
    return PostAPI(request, 'ClareInfoTechDMT/SenderRegistration');
};

export const SenderRegistrationSubmitOTP = async (request) => {
    return PostAPI(request, 'ClareInfoTechDMT/SenderRegistrationSubmitOTP');
};

export const DeleteBeneficiary = async (request) => {
    return PostAPI(request, 'ClareInfoTechDMT/DeleteBeneficiary');
};

export const ValidateDeleteBeneficiaryOTP = async (request) => {
    return PostAPI(request, 'ClareInfoTechDMT/ValidateDeleteBeneficiaryOTP');
};
