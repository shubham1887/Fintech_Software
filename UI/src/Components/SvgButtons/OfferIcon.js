import React from 'react';

import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';

const useStyles = makeStyles({
    ColorFill: {
        fill: '#fff'
    }
});

const OfferIcon = (props) => {
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
                viewBox="0 0 44.7 44.7"
                style={{
                    width: '32px'
                }}
            >
                <path
                    className={classesInline.ColorFill}
                    d="M38.5,7l-6.6,6.5c2.7,3.6,2.4,8.6-0.7,11.7L19.7,36.7L7.2,24l11.4-11.4c1.7-1.7,3.9-2.6,6.3-2.6
	c2,0,3.8,0.6,5.4,1.9l6.5-6.5C32.9,2,27.9,0,22.3,0C10,0,0,10,0,22.3s10,22.3,22.3,22.3s22.3-10,22.3-22.3C44.7,16.4,42.3,11,38.5,7
	z"
                />
                <path
                    className={classesInline.ColorFill}
                    d="M19.9,20.2c0.5,0,0.7-0.2,0.7-0.7c0-0.5-0.2-0.7-0.7-0.7c-0.2,0-0.7,0.3-0.7,0.7C19.2,20,19.4,20.2,19.9,20.2z"
                />
                <path
                    className={classesInline.ColorFill}
                    d="M20,27.3c-0.5,0-0.7,0.2-0.7,0.7s0.2,0.7,0.7,0.7c0.5,0,0.7-0.2,0.7-0.7S20.5,27.3,20,27.3z"
                />
                <path
                    className={classesInline.ColorFill}
                    d="M28.4,13.5c-1-0.7-2.3-1.1-3.6-1.1c-1.7,0-3.3,0.7-4.5,1.9L10.5,24l9.2,9.2l9.7-9.7c2.3-2.3,2.5-5.8,0.6-8.4
	l-1.4,1.4c0.3,1,0.1,2-0.7,2.7c-0.4,0.4-1.1,0.7-1.8,0.7c-0.6,0-1.2-0.1-1.8-0.7c-0.5-0.5-0.7-1.1-0.7-1.8s0.3-1.4,0.7-1.8
	c0.4-0.4,1.1-0.7,1.8-0.7c0.3,0,0.6,0.1,0.8,0.2L28.4,13.5z M18,17.7c0.5-0.4,1-0.7,1.8-0.7c0.7,0,1.4,0.3,1.8,0.7c1,1,1,2.7,0,3.7
	c-0.5,0.4-1,0.7-1.8,0.7c-0.6,0-1.2-0.3-1.8-0.7C17,20.3,17,18.7,18,17.7z M21.8,29.8c-0.5,0.4-1,0.7-1.8,0.7
	c-0.7,0-1.4-0.3-1.8-0.7c-1-1-1-2.7,0-3.7c0.5-0.4,1-0.7,1.8-0.7c0.7,0,1.4,0.3,1.8,0.7C22.8,27.2,22.8,28.9,21.8,29.8z M25.4,21.7
	l0.4,2.4l-11.5,1.7l-0.4-2.4L25.4,21.7z"
                />
            </svg>
        </>
    );
};

export default OfferIcon;
