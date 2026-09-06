import { PostAPI } from 'Components/Helper/Helpers';

export const AddSlider = async (request) => {
    return PostAPI(request, 'ServiceWiseSliderSetting/Insert');
};

export const UpdateSlider = async (request) => {
    return PostAPI(request, 'ServiceWiseSliderSetting/Update');
};

export const SelectSliders = async (request) => {
    return PostAPI(request, 'ServiceWiseSliderSetting/Select');
};

export const DeleteSlider = async (request) => {
    return PostAPI(request, 'ServiceWiseSliderSetting/Delete');
};

export const ActiveDeactiveSlider = async (request) => {
    return PostAPI(request, 'ServiceWiseSliderSetting/ActiveDeactive');
};

export const SelectSlidersByID = async (request) => {
    return PostAPI(request, 'ServiceWiseSliderSetting/SelectSlidersByID');
};
