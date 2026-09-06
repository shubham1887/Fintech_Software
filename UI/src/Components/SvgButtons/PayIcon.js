import React from 'react';

import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';

const useStyles = makeStyles({
    ColorFill: {
        fill: '#fff'
    }
});

const PayIcon = (props) => {
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
                        d="M18.9,0C8.5,0,0,8.5,0,18.9s8.5,18.9,18.9,18.9s18.9-8.5,18.9-18.9S29.3,0,18.9,0z M26.7,12.4H23
	c0.6,0.8,1.1,1.8,1.3,2.9h2.4v2.9h-2.4c-0.7,3.3-3.6,5.8-7.2,5.8h-1.1l6.5,6.5l-2.1,2.1L12,24v-2.9h5.2c1.9,0,3.5-1.2,4.1-2.9H12
	v-2.9h9.3c-0.6-1.7-2.2-2.9-4.1-2.9H12V9.4h14.7V12.4z"
                    />
                </g>
            </svg>
        </>
    );
};

export default PayIcon;
