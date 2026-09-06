import React from 'react';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
export const LandlineIcon = (props) => {
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
                d="M482,96H339.5c1.7-9.5,6.6-12.6,13.4-16.8c8.6-5.3,20.3-12.6,20.3-33.2c0-5.5-4.5-10-10-10s-10,4.5-10,10
	c0,9.5-3.4,11.6-10.8,16.2c-8.8,5.5-20.6,12.8-23,33.8H200V66c0-16.5-13.5-30-30-30H90c-16.5,0-30,13.5-30,30v30H30
	c-16.5,0-30,13.5-30,30v320c0,16.5,13.5,30,30,30h452c16.5,0,30-13.5,30-30V126C512,109.5,498.5,96,482,96L482,96z M80,66
	c0-5.5,4.5-10,10-10h80c5.5,0,10,4.5,10,10v320c0,5.5-4.5,10-10,10H90c-5.5,0-10-4.5-10-10V66z M492,446c0,5.5-4.5,10-10,10H30
	c-5.5,0-10-4.5-10-10V126c0-5.5,4.5-10,10-10h30v270c0,16.5,13.5,30,30,30h80c16.5,0,30-13.5,30-30V116h282c5.5,0,10,4.5,10,10V446z
	"
            />
            <path
                className={colorSelected}
                d="M426,171H266c-11,0-20,9-20,20s9,20,20,20h160c11,0,20-9,20-20S437,171,426,171z"
            />
            <path
                className={colorSelected}
                d="M256,281h20c5.5,0,10-4.5,10-10v-20c0-5.5-4.5-10-10-10h-20c-5.5,0-10,4.5-10,10v20
	C246,276.5,250.5,281,256,281z"
            />
            <path
                className={colorSelected}
                d="M336,281h20c5.5,0,10-4.5,10-10v-20c0-5.5-4.5-10-10-10h-20c-5.5,0-10,4.5-10,10v20
	C326,276.5,330.5,281,336,281z"
            />
            <path
                className={colorSelected}
                d="M416,281h20c5.5,0,10-4.5,10-10v-20c0-5.5-4.5-10-10-10h-20c-5.5,0-10,4.5-10,10v20
	C406,276.5,410.5,281,416,281z"
            />
            <path
                className={colorSelected}
                d="M256,341h20c5.5,0,10-4.5,10-10v-20c0-5.5-4.5-10-10-10h-20c-5.5,0-10,4.5-10,10v20
	C246,336.5,250.5,341,256,341z"
            />
            <path
                className={colorSelected}
                d="M336,341h20c5.5,0,10-4.5,10-10v-20c0-5.5-4.5-10-10-10h-20c-5.5,0-10,4.5-10,10v20
	C326,336.5,330.5,341,336,341z"
            />
            <path
                className={colorSelected}
                d="M416,341h20c5.5,0,10-4.5,10-10v-20c0-5.5-4.5-10-10-10h-20c-5.5,0-10,4.5-10,10v20
	C406,336.5,410.5,341,416,341z"
            />
            <path
                className={colorSelected}
                d="M256,401h20c5.5,0,10-4.5,10-10v-20c0-5.5-4.5-10-10-10h-20c-5.5,0-10,4.5-10,10v20
	C246,396.5,250.5,401,256,401z"
            />
            <path
                className={colorSelected}
                d="M336,401h20c5.5,0,10-4.5,10-10v-20c0-5.5-4.5-10-10-10h-20c-5.5,0-10,4.5-10,10v20
	C326,396.5,330.5,401,336,401z"
            />
            <path
                className={colorSelected}
                d="M416,401h20c5.5,0,10-4.5,10-10v-20c0-5.5-4.5-10-10-10h-20c-5.5,0-10,4.5-10,10v20
	C406,396.5,410.5,401,416,401z"
            />
        </svg>
    );
};
