import React, { useState, memo } from 'react';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

const AddmoneyAppBar = (props) => {
    const classes = commonStyle();

    const [colorSelected] = useState(classes.svg_white);

    return (
        <>
            <svg
                version="1.1"
                id="Layer_1"
                xmlns="http://www.w3.org/2000/svg"
                x="0px"
                y="0px"
                viewBox="0 0 512 512"
                style={{
                    enableBackground: 'new 0 0 512 512',
                    width: '36px'
                }}
            >
                <path
                    className={colorSelected}
                    d="M437,75C388.7,26.6,324.4,0,256,0S123.3,26.6,75,75C26.6,123.3,0,187.6,0,256s26.6,132.7,75,181
	c48.4,48.4,112.6,75,181,75s132.7-26.6,181-75c48.4-48.4,75-112.6,75-181S485.4,123.3,437,75L437,75z M415.8,415.8
	C373.1,458.5,316.4,482,256,482s-117.1-23.5-159.8-66.2S30,316.4,30,256S53.5,138.9,96.2,96.2S195.6,30,256,30
	s117.1,23.5,159.8,66.2S482,195.6,482,256S458.5,373.1,415.8,415.8z"
                />
                <g>
                    <path
                        className={colorSelected}
                        d="M349,142.3H175.5v34.5h61.2c22.5,0,41.6,14.4,48.7,34.5H175.5v34.5h109.9c-7.1,20.1-26.3,34.5-48.7,34.5h-61.2
		v34.5l101,101l24.4-24.4l-76.6-76.6h12.4c41.6,0,76.4-29.6,84.4-68.9H349v-34.5h-27.9c-2.6-12.7-8-24.4-15.5-34.5H349V142.3z"
                    />
                </g>
            </svg>
        </>
    );
};

export default memo(AddmoneyAppBar);
