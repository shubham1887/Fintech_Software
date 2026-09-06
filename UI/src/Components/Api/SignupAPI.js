import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'OutsideRegistration/Insert');
};

export const SelectState = async (request) => {
    return PostAPI(request, 'OutsideRegistration/SelectState');
};

export const SelectRole = async (request) => {
    return PostAPI(request, 'OutsideRegistration/SelectRoleForOutside');
};

export const SelectPlayStoreURL = async (request) => {
    return PostAPI(request, 'OutsideRegistration/SelectPlayStoreURL');
};

export const SelectRoleForSignUp = async (request) => {
    return PostAPI(request, 'OutsideRegistration/SelectRoleForSignUp');
};

export const SignUpSendOTP = async (request) => {
    return PostAPI(request, 'SignUp/GenerateSignUpOTP');
};

export const VerifySignUpOTP = async (request) => {
    return PostAPI(request, 'SignUp/VerifySignUpOTP');
};

export const SignUp = async (request) => {
    return PostAPI(request, 'SignUp/SignUp');
};

export const SelectReport = async (request) => {
    return PostAPI(request, 'SignUp/SelectReport');
};

export const Select = async (request) => {
    return PostAPI(request, 'SignUp/SelectForFailedSignup');
};