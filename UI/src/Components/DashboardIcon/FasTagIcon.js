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

const FasTagIcon = (props) => {
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
                        <path
                            className={colorSelected}
                            d="M305.9,365.6c-7.9,19.7-45.4,9.2-34.4-15.9C281.1,328,315.5,341.6,305.9,365.6L305.9,365.6z M478,362.6
	c-7.4,23.8-42.9,13.3-35.9-10.6C448.4,330.6,485.6,338,478,362.6L478,362.6z M439.6,260c4.8,5.3,7.5,16,10.4,22.9
	c3,7.3,6.8,15,9.2,22.2l2.5,7.8l-174.9-0.3l22.9-54.5c27-1.2,70-0.2,99.9-0.2C415.7,258.1,439.5,256.5,439.6,260L439.6,260z
	 M243.7,326.3l0.6,0.9l-0.4,77.9l18.3,0.1c0.2,13.4-0.5,21.9,2,34.2c8.6,3.5,2,6.2,19,6.2c10.4,0,14.4-0.9,18.4-7.8
	c2.8-6,2.2-8.8,2.1-15.6c0-2.9-0.2-6.1-0.2-9c0-6-1.2-6.2,2.7-7.9l140.9,0.4c-0.2,15.1-2.9,31.1,7,37.8c6.6,3,20.5,3.5,27,0
	c4.1-2.5,6.1-4.3,6.6-10.1c-0.1-7.7-1.6-21.4,1.1-28.1l17.1-0.7l-0.4-76.6c-2.6-6.1-5-10.3-10.4-12.9c-8.9-4.5-15.6,2.2-15.5-9.3
	c-2.4-2.7-2.5-3.8-4-7.8l-9.5-23.6c-2.2-5.2-10.6-28.7-12.9-31.4c-5.2-6.1-106.5-3-128.3-3c-26.2,0-26.6-3-32.1,10.8
	c-7.8,19.7-16.2,44-25,62.1C248.8,313,251.4,318.1,243.7,326.3L243.7,326.3z"
                        />
                        <path
                            className={colorNonSelected}
                            d="M156.6,481.9H33.8c-14.9,0-27.1-12.1-27.1-27.1V58.4c0-14.9,12.1-27.1,27.1-27.1h122.8
	c14.9,0,27.1,12.1,27.1,27.1v396.4C183.7,469.7,171.5,481.9,156.6,481.9z M33.8,54.6c-2.1,0-3.8,1.7-3.8,3.8v396.4
	c0,2.1,1.7,3.8,3.8,3.8h122.8c2.1,0,3.8-1.7,3.8-3.8V58.4c0-2.1-1.7-3.8-3.8-3.8H33.8z"
                        />
                        <path
                            className={colorNonSelected}
                            d="M116.5,230.2H74c-11.8,0-21.3-9.6-21.3-21.3V92.9c0-11.8,9.6-21.3,21.3-21.3h42.5c11.8,0,21.3,9.6,21.3,21.3
	l0,116C137.6,220.7,128.1,230.2,116.5,230.2z M74,89.5c-1.9,0-3.5,1.6-3.5,3.5v115.9c0,1.9,1.6,3.5,3.5,3.5h42.5
	c1.9,0,3.4-1.6,3.5-3.6V92.9c0-1.9-1.6-3.5-3.5-3.5H74z"
                        />
                        <path
                            className={colorNonSelected}
                            d="M179.5,345.5c-3.4,0-6.9-1.3-9.5-3.9c-5.2-5.2-5.2-13.7,0-18.9L422.8,69.9c5.2-5.2,13.7-5.2,18.9,0
	c5.2,5.2,5.2,13.7,0,18.9L189,341.6C186.3,344.2,182.9,345.5,179.5,345.5z"
                        />
                    </svg>
                </p>
                <h6>FasTag</h6>
            </div>
        </>
    );
};

export default FasTagIcon;
