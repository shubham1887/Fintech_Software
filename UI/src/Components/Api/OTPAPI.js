import { PostAPI } from 'Components/Helper/Helpers';

export const Send = async (request) => {
    return PostAPI(request, 'OTP/Send');
};
