import React, { useState } from 'react';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';

const useStyles = makeStyles({
    dashboardserviceicons: (props) => ({
        padding: '15px',
        borderRadius: '3px',
        width: '100%',
        '&:hover': {
            background: `${props.hoverColor} !important`,
            color: '#fff'
        }
    })
});

const PanCardIcon = (props) => {
    const propsInline = { hoverColor: PRIMARY_COLOR() };

    const classesInline = useStyles(propsInline);
    const classes = commonStyle();

    const [colorNonSelected, setColorNonSelected] = useState(
        classes.svg_non_selected
    );
    const [colorSelected, setColorSelected] = useState(classes.svg_selected);

    function MouseOver(event) {
        setColorSelected(classes.svg_white);
        setColorNonSelected(classes.svg_white);
    }
    function MouseOut(event) {
        setColorSelected(classes.svg_selected);
        setColorNonSelected(classes.svg_non_selected);
    }

    return (
        <>
            <div
                className={classesInline.dashboardserviceicons}
                onMouseOver={MouseOver}
                onMouseOut={MouseOut}
            >
                <p>
                    {/* <svg
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
                            className={colorNonSelected}
                            d="M528 32H48C21.5 32 0 53.5 0 80v16h576V80c0-26.5-21.5-48-48-48zM0 432c0 26.5 21.5 48 48 48h480c26.5 0 48-21.5 48-48V128H0v304zm352-232c0-4.4 3.6-8 8-8h144c4.4 0 8 3.6 8 8v16c0 4.4-3.6 8-8 8H360c-4.4 0-8-3.6-8-8v-16zm0 64c0-4.4 3.6-8 8-8h144c4.4 0 8 3.6 8 8v16c0 4.4-3.6 8-8 8H360c-4.4 0-8-3.6-8-8v-16zm0 64c0-4.4 3.6-8 8-8h144c4.4 0 8 3.6 8 8v16c0 4.4-3.6 8-8 8H360c-4.4 0-8-3.6-8-8v-16zM176 192c35.3 0 64 28.7 64 64s-28.7 64-64 64-64-28.7-64-64 28.7-64 64-64zM67.1 396.2C75.5 370.5 99.6 352 128 352h8.2c12.3 5.1 25.7 8 39.8 8s27.6-2.9 39.8-8h8.2c28.4 0 52.5 18.5 60.9 44.2 3.2 9.9-5.2 19.8-15.6 19.8H82.7c-10.4 0-18.8-10-15.6-19.8z"
                        />
                    </svg> */}
                </p>
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
                    <g>
                        <path
                            className={colorNonSelected}
                            d="M448,72H64c-35.3,0-64,28.7-64,64v240c0,35.3,28.7,64,64,64h384c35.3,0,64-28.7,64-64V136
		C512,100.7,483.3,72,448,72L448,72z M480,376c0,17.6-14.4,32-32,32l-384,0c-17.6,0-32-14.4-32-32V136c0-17.6,14.4-32,32-32h384
		c17.6,0,32,14.4,32,32L480,376z"
                        />
                        <path
                            className={colorNonSelected}
                            d="M176,248c26.5,0,48-21.5,48-48s-21.5-48-48-48s-48,21.5-48,48C128,226.5,149.5,248,176,248z M176,184
		c8.8,0,16,7.2,16,16c0,8.8-7.2,16-16,16c-8.8,0-16-7.2-16-16C160,191.2,167.2,184,176,184z"
                        />
                        <path
                            className={colorNonSelected}
                            d="M176,264c-45.7,0-85,31.4-95.6,76.4c-2,8.6,3.3,17.2,11.9,19.2c8.6,2,17.2-3.3,19.2-11.9
		c7.1-30.4,33.6-51.6,64.4-51.6c30.8,0,57.3,21.2,64.4,51.7c2,8.7,10.7,13.9,19.2,11.9c8.6-2,13.9-10.6,11.9-19.2
		C261,295.4,221.7,264,176,264L176,264z"
                        />
                        <path
                            className={colorSelected}
                            d="M416,200H304c-8.8,0-16,7.2-16,16c0,8.8,7.2,16,16,16h112c8.8,0,16-7.2,16-16C432,207.2,424.9,200,416,200z"
                        />
                        <path
                            className={colorNonSelected}
                            d="M384,280h-80c-8.8,0-16,7.2-16,16s7.2,16,16,16h80c8.8,0,16-7.2,16-16S392.9,280,384,280z"
                        />
                    </g>
                </svg>
                <h6>Pan Card</h6>
            </div>
        </>
    );
};

export default PanCardIcon;
