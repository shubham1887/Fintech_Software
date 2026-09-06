import { PostAPI } from 'Components/Helper/Helpers';
export const VersionHistorySelect = async (request) => {
    return PostAPI(request, 'VersionHistory/VersionHistorySelect');
};
