import { PostAPI } from 'Components/Helper/Helpers';
export const UserOnbordingRegistrtionOTP = async (request) => {
    return PostAPI(
        request,
        'InstantPayUserOnbording/UserOnbordingRegistrtionOTP'
    );
};
export const UserOnbordingRegistrtion = async (request) => {
    return PostAPI(request, 'InstantPayUserOnbording/UserOnbordingRegistrtion');
};
export const SelectOutletID = async (request) => {
    return PostAPI(request, 'InstantPayUserOnbording/SelectOutletID');
};

export const ChangeMobileNumber = async (request) => {
    return PostAPI(request, 'InstantPayUserOnbording/ChangeMobileNumber');
};

export const ChangeMobileNumberVerify = async (request) => {
    return PostAPI(request, 'InstantPayUserOnbording/ChangeMobileNumberVerify');
};
