import { PostAPI } from 'Components/Helper/Helpers';

export const VerifyPan = async (request) => {
    return PostAPI(request, 'eKYC/VerifyPan');
};
export const eKYCInitiate = async (request) => {
    return PostAPI(request, 'eKYC/eKYCInitiate');
};
export const eKYCValidate = async (request) => {
    return PostAPI(request, 'eKYC/eKYCValidate');
};

export const PaysprinteKYCInitiate = async (request) => {
    return PostAPI(request, 'eKYC/PaysprinteKYCInitiate');
};
export const PaysprinteKYCValidate = async (request) => {
    return PostAPI(request, 'eKYC/PaysprinteKYCValidate');
};
export const GSTVerification = async (request) => {
    return PostAPI(request, 'eKYC/GSTVerification');
};
export const PanVerification = async (request) => {
    return PostAPI(request, 'eKYC/PanVerification');
};
export const SelectByUserID = async (request) => {
    return PostAPI(request, 'eKYCLog/SelectByUserID');
};
export const Select = async (request) => {
    return PostAPI(request, 'eKYCLog/Select');
};
export const PDRSeKYCInitiate = async (request) => {
    return PostAPI(request, 'eKYC/PDRSeKYCInitiate');
};
export const PDRSeKYCValidate = async (request) => {
    return PostAPI(request, 'eKYC/PDRSeKYCValidate');
};
export const QuickeKYCInitiate = async (request) => {
    return PostAPI(request, 'eKYC/QuickeKYCInitiate');
};
export const QuickeKYCValidate = async (request) => {
    return PostAPI(request, 'eKYC/QuickeKYCValidate');
};

export const InstantPayKYCInitiate = async (request) => {
    return PostAPI(request, 'eKYC/InstantPayKYCInitiate');
};
export const InstantPayKYCValidate = async (request) => {
    return PostAPI(request, 'eKYC/InstantPayKYCValidate');
};

export const CashfreeEKYCInitiate = async (request) => {
    return PostAPI(request, 'eKYC/CashfreeEKYCInitiate');
};
export const CashfreeEKYCValidate = async (request) => {
    return PostAPI(request, 'eKYC/CashfreeEKYCValidate');
};

export const SignupeKYCInitiate = async (request) => {
    return PostAPI(request, 'SignUpEkyc/SignupeKYCInitiate');
};

export const SignupKYCValidate = async (request) => {
    return PostAPI(request, 'SignUpEkyc/SignupKYCValidate');
};


export const YesBankEkycInitiate = async (request) => {
    return PostAPI(request, 'eKYC/YesBankEkycInitiate');
};

export const YesBankEkycValidate = async (request) => {
    return PostAPI(request, 'eKYC/YesBankEkycValidate');
};

export const YesBankBioMatricVerification = async (request) => {
    return PostAPI(request, 'eKYC/YesBankBioMatricVerification');
};