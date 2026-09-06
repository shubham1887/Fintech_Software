import { PostAPI } from 'Components/Helper/Helpers';

export const Initiate = async (request) => {
    return PostAPI(request, 'BusBooking/Initiate');
};
