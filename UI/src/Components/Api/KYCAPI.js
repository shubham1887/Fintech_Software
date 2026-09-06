import { PostAPI } from 'Components/Helper/Helpers';

export const Upload = async (request) => {
    return PostAPI(request, 'KYC/Upload');
};

export const Select = async (request) => {
    return PostAPI(request, 'KYC/Select');
};

export const SelectDocuments = async (request) => {
    return PostAPI(request, 'KYC/selectDocumentsByUserID');
};

export const Approve = async (request) => {
    return PostAPI(request, 'KYC/Approve');
};
export const SelectDashboardKyc = async (request) => {
    return PostAPI(request, 'KYC/SelectKycPendingCount');
};
export const SelectRequired = async (request) => {
    return PostAPI(request, 'KycDocumentMaster/Select');
};
export const IsRequiredOnOff = async (request) => {
    return PostAPI(request, 'KycDocumentMaster/RequiredOnOff');
};
export const selectDocType = async (request) => {
    return PostAPI(request, 'KycDocumentMaster/SelectRequired');
};

export const selectDocUrl = async (request) => {
    return PostAPI(request, 'KYC/selectByUserIDwithDocumentType');
};
export const SelectPending = async (request) => {
    return PostAPI(request, 'KYC/SelectPending');
};
export const SelectBYID = async (request) => {
    return PostAPI(request, 'KYC/SelectByID');
};
export const SelectActiveDocUserwise = async (request) => {
    return PostAPI(request, 'KYC/SelectActiveDocUserwise');
};
