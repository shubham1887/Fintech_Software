import React, { useEffect, useState } from 'react';
import { Button } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';

const useStyles = makeStyles({
    dashboardserviceicons: (props) => ({
        padding: '15px',
        width: '100%',
        borderRadius: '3px',
        '&:hover': {
            background: `${props.hoverColor} !important`,
            color: '#fff'
        }
    })
});

const PayoutIcon = (props) => {
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
                                className={colorSelected}
                                d="M256,170.1c-29.5,0-53.3,23.9-53.3,53.3s23.9,53.3,53.3,53.3s53.3-23.9,53.3-53.3S285.5,170.1,256,170.1
		L256,170.1z M256,255.4c-17.6,0-32-14.4-32-32s14.4-32,32-32c17.7,0,32,14.4,32,32S273.7,255.4,256,255.4z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M364.3,106.1V67.4c0-22.3-18.1-40.4-40.4-40.4c-7.3,0-14,1.9-19.9,5.3C300.2,13.8,283.9,0,264.4,0
		c-20.4,0-37.3,15.1-40,34.8c-6.7-5-15-7.9-24-7.9C178.1,26.9,160,45,160,67.4v21c-6.6-4.8-14.8-7.6-23.5-7.6
		c-17,0-31.5,10.4-37.5,25.3H32v234.7h64v29.9C96,448.7,159.3,512,237.4,512c55.8,0,104-32.3,127-79.2l46.4-92l69.2,0V106.1
		L364.3,106.1z M304.8,67.4c0-10.5,8.6-19.1,19.1-19.1c10.5,0,19.1,8.6,19.1,19.1v38.7h-38.2V67.4z M245.3,40.4
		c0-10.5,8.6-19.1,19.1-19.1c10.5,0,19.1,8.6,19.1,19.1v65.7h-38.2V40.4z M181.3,67.4c0-10.5,8.6-19.1,19.1-19.1
		c10.5,0,19.1,8.6,19.1,19.1v38.7h-38.2V67.4z M136.5,102.2c4.3,0,8.4,1.5,11.6,3.9h-23.1C128.1,103.6,132.1,102.2,136.5,102.2
		L136.5,102.2z M416.5,282L416.5,282l-0.1,0.2l-36.5,72.3l-30,59.6c-5.9,15.5-15.7,29.4-27.8,41.4c-22.7,22.7-52.8,35.1-84.9,35.1
		c-32.1,0-62.2-12.5-84.9-35.1c-22.7-22.7-35.1-52.8-35.1-84.9h0v-29.9h191.3l-11.3,21.9h24l42.7-82.6l10.2-19.8
		c4.1-8,12.2-12.9,21.2-12.9c3.8,0,7.5,0.9,10.9,2.6c5.6,2.9,9.8,7.9,11.8,13.9C420,269.9,419.4,276.4,416.5,282L416.5,282z
		 M336.2,287.4H112.9c-5.4-12.3-15.3-22.2-27.5-27.5V187c12.3-5.4,22.2-15.3,27.5-27.5h286.2c5.4,12.3,15.3,22.2,27.5,27.5v51.6
		c-3.1-3-6.6-5.5-10.6-7.6c-6.6-3.4-13.7-5-20.6-5c-16.3,0-32.1,8.9-40.2,24.5L336.2,287.4z M458.7,319.4h-37.1l14-27.6
		c2.3-4.5,3.8-9.2,4.5-14c2.6-0.7,5.3-1,8-1V170.1c-17.7,0-32-14.3-32-32H96c0,17.7-14.3,32-32,32v106.7c17.7,0,32,14.3,32,32h229.2
		l-5.5,10.7H53.3v-192h405.3L458.7,319.4z"
                            />
                        </g>
                    </svg>
                </p>
                <h6>{props.name}</h6>
            </div>
        </>
    );
};

export default PayoutIcon;
