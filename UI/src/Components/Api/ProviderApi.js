import { PostAPI } from 'Components/Helper/Helpers';

export const Add = async (request) => {
    return PostAPI(request, 'ProviderMaster/Insert');
};

export const Activate = async (request) => {
    return PostAPI(request, 'ProviderMaster/ActiveDeactive');
};

export const Update = async (request) => {
    return PostAPI(request, 'ProviderMaster/Update');
};

export const Select = async (request) => {
    return PostAPI(request, 'ProviderMaster/Select');
};
export const SelectByID = async (request) => {
    return PostAPI(request, 'ProviderMaster/SelectByID');
};
export const SelectForDropDown = async (request) => {
    return PostAPI(request, 'ProviderMaster/SelectForDropDown');
};
export const SelectForRealAPIDropDown = async (request) => {
    return PostAPI(request, 'ProviderMaster/SelectForRealAPIDropDown');
};
export const SelectForViewAPIDropDown = async (request) => {
    return PostAPI(request, 'ProviderMaster/SelectForViewAPIDropDown');
};

export const HLROnOff = async (request) => {
    return PostAPI(request, 'ProviderMaster/HLROnOff');
};
export const Balance = async (request) => {
    return PostAPI(request, 'ProviderMaster/CheckProviderBalance');
};
export const CheckAllProviderBalance = async (request) => {
    return PostAPI(request, 'ProviderMaster/CheckAllProviderBalance');
};
export const Delete = async (request) => {
    return PostAPI(request, 'ProviderMaster/Delete');
};
export const SelectSampleProvider = async (request) => {
    return PostAPI(request, 'ProviderDetailsSample/Select');
};

export const SelectBySampleID = async (request) => {
    return PostAPI(request, 'ProviderDetailsSample/SelectByID');
};
export const SelectDeveloperForDropDown = async (request) => {
    return PostAPI(request, 'ProviderDetailsSample/SelectDeveloperForDropDown');
};

export const AddSampleProvider = async (request) => {
    return PostAPI(
        request,
        'ProviderDetailsSample/AddSampleProviderToProviderDetails'
    );
};

export const SelectSampleURLByDeveloper = async (request) => {
    return PostAPI(request, 'ProviderDetailsSample/SelectSampleURLByDeveloper');
};

export const SelectProviderForBulkSequence = async (request) => {
    return PostAPI(request, 'OperatorSequence/SelectProviderForBulkSequence');
};

export const SelectDMTAsPayoutAPI = async (request) => {
    return PostAPI(request, 'ProviderMaster/SelectDMTAsPayoutAPI');
};

export const SelectDMTAPI = async (request) => {
    return PostAPI(request, 'ProviderMaster/SelectDMTProvider');
};

export const SelectProviderUniqueKey = async (request) => {
    return PostAPI(request, 'ProviderMaster/SelectProviderUniqueKey');
};

export const SelectProviderUniqueKeyForAllCategory = async (request) => {
    return PostAPI(
        request,
        'ProviderMaster/SelectProviderUniqueKeyForAllCategory'
    );
};
