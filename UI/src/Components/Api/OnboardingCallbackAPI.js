import { PostAPI } from 'Components/Helper/Helpers';
export const SelectReport = async (request) => {
    return PostAPI(request, 'OnboardingUser/SelectReport');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'OnboardingUser/SelectByID');
};
export const Update = async (request) => {
    return PostAPI(request, 'OnboardingUser/Update');
};
export const ApproveOnboarding = async (request) => {
    return PostAPI(request, 'OnboardingUser/ApproveOnboarding');
};
export const ManualInsert = async (request) => {
    return PostAPI(request, 'OnboardingUser/ManualInsert');
};


