import { PostAPI } from 'Components/Helper/Helpers';

export const Select = async (request) => {
    return PostAPI(request, 'XMPP/Select');
};

export const Reconnect = async (request) => {
    return PostAPI(request, 'XMPP/Reconnect');
};
