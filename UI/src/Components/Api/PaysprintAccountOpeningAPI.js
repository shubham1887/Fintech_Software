import { PostAPI } from 'Components/Helper/Helpers';

export const AccountOpenGenerateURL = async (request) => {
    return PostAPI(request, 'PaysprintAccountOpening/AccountOpenGenerateURL');
};

export const UserOnbording = async (request) => {
    return PostAPI(request, 'PaysprintAccountOpening/UserOnbording');
};
