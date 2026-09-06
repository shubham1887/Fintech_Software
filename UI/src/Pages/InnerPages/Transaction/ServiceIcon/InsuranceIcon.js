import React from 'react';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
export const InsuranceIcon = (props) => {
    const { serviceID, value } = props;
    const classes = commonStyle();
    let colorNonSelected = classes.svg_non_selected;
    let colorSelected = classes.svg_selected;
    if (serviceID !== value) {
        colorSelected = classes.svg_non_selected;
    }

    return (
        <svg
            version="1.1"
            id="Layer_1"
            xmlns="http://www.w3.org/2000/svg"
            x="0px"
            y="0px"
            viewBox="0 0 512 512"
            style={{ enableBackground: 'new 0 0 512 512' }}
        >
            <path
                className={colorSelected}
                d="M301,467c0,8.3-6.7,15-15,15s-15-6.7-15-15V292.6l-15,15.8l-15-15.8V467c0,24.8,20.2,45,45,45s45-20.2,45-45
	v-30h-30V467z"
            />
            <g>
                <path
                    className={colorNonSelected}
                    d="M271,292.6c9.8-8.4,28.7-21.6,45-21.6c19.4,0,42.2,18.5,49.4,25.6l10.6,10.5l10.6-10.5
		c7.2-7.1,30.1-25.6,49.4-25.6c26,0,54.1,13.3,54.5,13.5l21.5,10.4V271c0-135.3-106.3-233.8-241-240.6V0h-30v30.4
		C106.3,37.2,0,135.8,0,271v23.8l21.5-10.3C21.8,284.4,49.9,271,76,271c19.4,0,42.2,18.5,49.4,25.6l10.6,10.5l10.6-10.5
		c7.2-7.1,30.1-25.6,49.4-25.6c16.4,0,35.2,13.2,45,21.6l15,15.8L271,292.6z M152.2,254.8c4.6-61.9,32.1-171.3,88.8-192v192.7
		c-12.5-7.6-28.4-14.5-45-14.5C179.9,241,164.5,247.5,152.2,254.8z M271,255.5V62.8c57,20.8,84.2,130.3,88.8,192
		c-12.3-7.3-27.7-13.8-43.8-13.8C299.4,241,283.5,247.9,271,255.5z M480.9,248.7c-12.5-4-28.7-7.8-44.9-7.8
		c-17.1,0-33.4,7.3-46,15.1c-4.1-61.7-22.8-137.9-62-186C412.2,94.8,472,162.6,480.9,248.7L480.9,248.7z M184,70.1
		c-39.2,48.1-57.8,124.3-62,186c-12.7-7.9-28.9-15.1-46-15.1c-16.2,0-32.4,3.8-44.9,7.8C40,162.6,99.8,94.8,184,70.1z"
                />
            </g>
        </svg>
    );
};
