import React from 'react';

import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';

const useStyles = makeStyles({
    ColorFill: {
        fill: '#fff'
    }
});

const InfoIcon = (props) => {
    const propsInline = { hoverColor: PRIMARY_COLOR() };

    const classesInline = useStyles(propsInline);

    return (
        <>
            <svg
                version="1.1"
                id="Layer_1"
                xmlns="http://www.w3.org/2000/svg"
                x="0px"
                y="0px"
                viewBox="0 0 37.8 37.8"
                style={{
                    width: '32px'
                }}
            >
                <g>
                    <path
                        className={classesInline.ColorFill}
                        d="M18.9,9C13.4,9,9,13.4,9,18.9s4.5,9.9,9.9,9.9c5.5,0,9.9-4.5,9.9-9.9S24.4,9,18.9,9z M20.4,25.3
		c0,0.8-0.7,1.5-1.5,1.5s-1.5-0.7-1.5-1.5v-7.2c0-0.8,0.7-1.5,1.5-1.5s1.5,0.7,1.5,1.5V25.3z M18.9,14.8c-1,0-1.9-0.8-1.9-1.9
		c0-1,0.8-1.9,1.9-1.9s1.9,0.8,1.9,1.9C20.8,14,19.9,14.8,18.9,14.8z"
                    />
                    <path
                        className={classesInline.ColorFill}
                        d="M18.9,0C8.5,0,0,8.5,0,18.9s8.5,18.9,18.9,18.9s18.9-8.5,18.9-18.9S29.3,0,18.9,0z M18.9,32.1
		c-7.3,0-13.2-5.9-13.2-13.2c0-7.3,5.9-13.2,13.2-13.2s13.2,5.9,13.2,13.2C32.1,26.2,26.2,32.1,18.9,32.1z"
                    />
                </g>
            </svg>
        </>
    );
};

export default InfoIcon;
