import { PostAPI } from 'Components/Helper/Helpers';

export const SelectBank = async (request) => {
    return PostAPI(request, 'NPCIBank/Select');
};
