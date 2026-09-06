import { makeStyles } from '@material-ui/core/styles';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PRIMARY_COLOR } from 'Config';
import React, { useState } from 'react';

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

const BUSIcon = (props) => {
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
                                d="M336,85.3H176c-41.6,0-74.7,33.1-74.7,74.7v83.2c0,18.1,13.9,32,32,32h245.3c18.1,0,32-13.9,32-32l0-83.2
		C410.7,118.4,377.6,85.3,336,85.3L336,85.3z M389.3,243.2c0,6.4-4.3,10.7-10.7,10.7H133.3c-6.4,0-10.7-4.3-10.7-10.7l0-83.2
		c0-29.9,23.5-53.3,53.3-53.3h160c29.9,0,53.3,23.5,53.3,53.3L389.3,243.2z"
                            />
                            <path
                                className={colorSelected}
                                d="M146.1,301.9c-26.7,0-48,21.3-48,48s21.3,48,48,48s48-21.3,48-48S171.7,301.9,146.1,301.9L146.1,301.9z
		 M146.1,376.5c-14.9,0-26.7-11.7-26.7-26.7c0-14.9,11.7-26.7,26.7-26.7c14.9,0,26.7,11.7,26.7,26.7
		C172.8,364.8,160,376.5,146.1,376.5L146.1,376.5z"
                            />
                            <path
                                className={colorSelected}
                                d="M365.9,301.9c-26.7,0-48,21.3-48,48s21.3,48,48,48s48-21.3,48-48S392.5,301.9,365.9,301.9L365.9,301.9z
		 M365.9,376.5c-14.9,0-26.7-11.7-26.7-26.7c0-14.9,11.7-26.7,26.7-26.7c14.9,0,26.7,11.7,26.7,26.7
		C392.5,364.8,380.8,376.5,365.9,376.5z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M296.5,316.8h-80c-6.4,0-10.7,4.3-10.7,10.7c0,6.4,4.3,10.7,10.7,10.7h80c6.4,0,10.7-4.3,10.7-10.7
		C307.2,322.1,301.9,316.8,296.5,316.8z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M296.5,361.6h-80c-6.4,0-10.7,4.3-10.7,10.7c0,6.4,4.3,10.7,10.7,10.7h80c6.4,0,10.7-4.3,10.7-10.7
		C307.2,365.9,301.9,361.6,296.5,361.6z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M448,84.3C426.7,35.2,377.6,0,320,0H192C134.4,0,85.3,35.2,64,84.3c-35.2,0-64,28.8-64,64v49.1
		c0,6.4,4.3,10.7,10.7,10.7s10.7-4.3,10.7-10.7v-49.1c0-21.3,16-38.4,36.3-41.6c-2.1,10.7-4.3,21.3-4.3,32v266.7
		c0,18.1,13.9,32,32,32h12.8v27.7c0,25.6,21.3,46.9,46.9,46.9s46.9-21.3,46.9-46.9v-27.7h126.9v27.7c0,25.6,21.3,46.9,46.9,46.9
		c25.6,0,46.9-21.3,46.9-46.9l0-27.7h13.9c18.1,0,32-13.9,32-32V138.7c0-10.7-1.1-22.4-4.3-32c20.3,3.2,36.3,20.3,36.3,41.6v49.1
		c0,6.4,4.3,10.7,10.7,10.7c6.4,0,10.7-4.3,10.7-10.7v-49.1C512,113.1,483.2,84.3,448,84.3L448,84.3z M171.7,465.1
		c0,13.9-11.7,25.6-25.6,25.6c-13.9,0-25.6-11.7-25.6-25.6v-27.7h51.2V465.1z M392.5,465.1c0,13.9-11.7,25.6-25.6,25.6
		s-25.6-11.7-25.6-25.6v-27.7h51.2V465.1z M437.3,405.3c0,6.4-4.3,10.7-10.7,10.7H85.3c-6.4,0-10.7-4.3-10.7-10.7l0-266.7
		c0-65.1,52.3-117.3,117.3-117.3h128c65.1,0,117.3,52.3,117.3,117.3L437.3,405.3z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M214.4,62.9h83.2c6.4,0,10.7-4.3,10.7-10.7s-4.3-10.7-10.7-10.7h-83.2c-6.4,0-10.7,4.3-10.7,10.7
		C203.7,58.7,209.1,62.9,214.4,62.9z"
                            />
                        </g>
                    </svg>
                </p>
                <h6>{props.name}</h6>
            </div>
        </>
    );
};

export default BUSIcon;
