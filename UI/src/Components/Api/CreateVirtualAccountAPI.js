import { PostAPI } from 'Components/Helper/Helpers';

export const VitrualAccount = async (request) => {
    return PostAPI(request, 'EaseBuzz/ViatualAccountCreate');
};