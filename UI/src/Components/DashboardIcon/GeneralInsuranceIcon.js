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

const GeneralInsuranceIcon = (props) => {
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
                            <g>
                                <path
                                    className={colorNonSelected}
                                    d="M425.3,70.2c-58.5,0-114.8-21.5-158.4-60.5L256,0l-10.9,9.7c-43.6,39-99.9,60.5-158.4,60.5H34v115.3
			c0,66.8,19,131.9,54.9,188.2s86.9,101,147.4,129.1L256,512l19.6-9.1c60.6-28.2,111.5-72.8,147.5-129.1
			C459,317.4,478,252.3,478,185.6V70.2H425.3z M448,185.6c0,61.1-17.4,120.5-50.2,172c-32.8,51.5-79.4,92.3-134.8,118.1l-7,3.2
			l-7-3.2c-55.4-25.8-102-66.6-134.8-118.1c-32.8-51.5-50.2-111-50.2-172v-85.3h22.6c61.9,0,121.6-21.3,169.3-60.3
			c47.7,39,107.4,60.3,169.3,60.3H448L448,185.6L448,185.6z"
                                />
                            </g>
                        </g>
                        <path
                            className={colorSelected}
                            d="M281.7,376.4c0,4.7-3.8,8.6-8.6,8.6s-8.6-3.8-8.6-8.6v-99.5l-8.6,9l-8.6-9v99.5c0,14.2,11.5,25.7,25.7,25.7
	c14.2,0,25.7-11.5,25.7-25.7v-17.1h-17.1V376.4z"
                        />
                        <g>
                            <path
                                className={colorSelected}
                                d="M264.6,276.9c5.6-4.8,16.4-12.3,25.7-12.3c11.1,0,24.1,10.6,28.2,14.6l6,6l6-6c4.1-4.1,17.2-14.6,28.2-14.6
		c14.8,0,30.9,7.6,31.1,7.7l12.3,5.9v-13.6c0-77.2-60.7-133.4-137.5-137.3v-17.4h-17.1v17.4c-76.9,3.9-137.5,60.2-137.5,137.3v13.6
		l12.3-5.9c0.2-0.1,16.2-7.7,31.1-7.7c11.1,0,24.1,10.6,28.2,14.6l6,6l6-6c4.1-4.1,17.2-14.6,28.2-14.6c9.4,0,20.1,7.5,25.7,12.3
		l8.6,9L264.6,276.9z M196.8,255.3c2.6-35.3,18.3-97.8,50.7-109.6v110c-7.1-4.3-16.2-8.3-25.7-8.3
		C212.6,247.4,203.8,251.1,196.8,255.3z M264.6,255.7v-110c32.5,11.9,48.1,74.4,50.7,109.6c-7-4.2-15.8-7.9-25-7.9
		C280.8,247.4,271.7,251.4,264.6,255.7z M384.4,251.8c-7.1-2.3-16.4-4.5-25.6-4.5c-9.8,0-19.1,4.2-26.3,8.6
		c-2.3-35.2-13-78.7-35.4-106.2C345.2,164,379.3,202.7,384.4,251.8L384.4,251.8z M214.9,149.9c-22.4,27.5-33,70.9-35.4,106.2
		c-7.2-4.5-16.5-8.6-26.3-8.6c-9.2,0-18.5,2.2-25.6,4.5C132.7,202.7,166.9,164,214.9,149.9z"
                            />
                        </g>
                    </svg>
                </p>
                <h6>General Insurance</h6>
            </div>
        </>
    );
};

export default GeneralInsuranceIcon;
