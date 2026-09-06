import { PostAPI } from 'Components/Helper/Helpers';

export const DailyAuthenticationSelect = async (request) => {
    return PostAPI(request, 'DailyAuthentication/SelectLast2FAByUserID');
};
