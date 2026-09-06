import { PostAPI } from 'Components/Helper/Helpers';

export const AddSlider = async (request) => {
    return PostAPI(request, 'SliderMaster/Insert');
};

export const UpdateSlider = async (request) => {
    return PostAPI(request, 'SliderMaster/Update');
};

export const SelectSliders = async (request) => {
    return PostAPI(request, 'SliderMaster/Select');
};

export const SelectSliderByID = async (request) => {
    return PostAPI(request, 'SliderMaster/SelectByID');
};

export const DeleteSlider = async (request) => {
    return PostAPI(request, 'SliderMaster/Delete');
};

export const SelectPublicSliders = async (request) => {
    return PostAPI(request, 'PublicSliderMaster/Select');
};
