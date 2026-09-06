import { PostAPI } from 'Components/Helper/Helpers';

export const SendPushNotification = async (request) => {
    return PostAPI(request, 'PushNotifications/SendPushNotification');
};
