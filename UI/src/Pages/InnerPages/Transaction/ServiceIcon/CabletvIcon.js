import React from 'react';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
export const CabletvIcon = (props) => {
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
                className={colorNonSelected}
                d="M121,422h90v-90h75c24.8,0,45,20.2,45,45v15h30V152h-30v15c0,24.8-20.2,45-45,45H91v120h30V422z M181,392h-30
	v-60h30V392z M331,226.9V317c-12.5-9.4-28.1-15-45-15h-45v-60h45C302.9,242,318.5,236.4,331,226.9L331,226.9z M121,242h90v60h-90
	V242z"
            />
            <path
                className={colorSelected}
                d="M383.8,329.6l16.7-25l45,30l-16.6,25L383.8,329.6z"
            />
            <path
                className={colorSelected}
                d="M383.7,214.4l45-30l16.6,25l-45,30L383.7,214.4z"
            />
            <path className={colorSelected} d="M392,257h60v30h-60V257z" />
            <path
                className={colorNonSelected}
                d="M512,92H305.5l48.8-32.5l-16.6-25l-82.6,55.1L82.9,0L69.1,26.6L194.7,92H0v390h61v30h30v-30h330v30h30v-30h61
	V92z M482,452c-87.7,0-404.1,0-452,0V122h452V452z"
            />
        </svg>
    );
};
