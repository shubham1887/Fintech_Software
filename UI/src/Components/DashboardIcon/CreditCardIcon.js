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

const CreditCardIcon = (props) => {
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
                            className={colorNonSelected}
                            d="M450.2,450H37.8c-10,0-19.6-4-26.7-11C4,432,0,422.4,0,412.4V202.1c0-10.3,4-20.1,11.1-27.4
		c7.1-7.3,16.7-11.3,26.7-11.3h255.9c4.2,0,7.9,2.8,8.9,6.9c1.4,6-3,11.3-8.6,11.3H37.3c-5.3,0-10.3,2-14.1,5.7
		c-3.8,3.7-5.9,8.7-6,14v211.5c0,5.3,2.1,10.5,5.9,14.2c3.8,3.8,8.9,5.9,14.2,5.9h412.9c0,0,10.4-2.2,14.2-6c3.8-3.7,5.9-8.8,6-14.2
		V240.4c-0.1-4.9,3.8-8.8,8.7-8.8c4.9,0,8.8,4,9.5,8.8v172.3c-0.7,10-4.7,19.6-11.8,26.6c-7.1,7.1-16.7,11-26.7,11L450.2,450z"
                        />
                        <path
                            className={colorNonSelected}
                            d="M398.5,287.2C368.6,287,340,275,319,253.8c-21-21.2-32.7-49.9-32.6-79.8c0.1-29.9,12.1-58.5,33.3-79.5
		c21.2-21.1,49.9-32.8,79.8-32.8c29.9,0.1,58.5,12,79.6,33.2c21.1,21.2,32.9,49.8,32.9,79.7c-0.2,30-12.2,58.7-33.5,79.8
		C457.2,275.4,428.5,287.2,398.5,287.2L398.5,287.2z M398.5,79.4c-25.2,0.2-49.3,10.4-67,28.3c-17.7,17.9-27.6,42.1-27.4,67.3
		c0.1,25.2,10.2,49.3,28.1,67c17.9,17.7,42.1,27.7,67.2,27.6c25.2-0.1,49.3-10.1,67.1-27.9c17.8-17.8,27.8-42,27.8-67.2
		c-0.2-25.3-10.3-49.5-28.3-67.3C448.1,89.3,423.8,79.4,398.5,79.4L398.5,79.4z"
                        />
                        <g>
                            <path
                                className={colorSelected}
                                d="M389.7,231.3c0-17.2,0-96.3,0-115.9c0-3.9,2.4-7.4,6.2-8.6c6.3-1.9,12,2.6,12,8.4v116.2
			c0,0.8-0.1,1.7-0.3,2.5c-1.8,5.6-8.3,8.1-13.6,5.1C391.2,237.4,389.7,234.4,389.7,231.3z"
                            />
                            <path
                                className={colorSelected}
                                d="M456.6,182.5H340.7c-4.2,0-7.9-2.8-8.9-6.9c-1.4-6,3-11.3,8.6-11.3h115.9c4.2,0,7.9,2.8,8.9,6.9
			C466.6,177.2,462.2,182.5,456.6,182.5z"
                            />
                        </g>
                        <path
                            className={colorNonSelected}
                            d="M156.6,366.3h-72c-4.5,0-8.8-1.8-12-5c-3.2-3.2-5-7.6-5-12.1v-71.8c0-4.5,1.8-8.9,5-12.1c3.2-3.2,7.5-5,12-5
		h71.7c2.2,0,4.4,0.3,6.5,1.2c6.8,2.7,10.8,9.1,10.8,15.9v71.8c0,4.5-1.8,8.9-5,12.1C165.5,364.4,161.1,366.3,156.6,366.3
		L156.6,366.3z M84.8,349.1h70.7v-70.7H84.8V349.1z"
                        />
                        <path
                            className={colorNonSelected}
                            d="M325.9,303.7H212c-4.2,0-7.9-2.8-8.9-6.9c-1.4-6,3-11.3,8.6-11.3h114.5c4.2,0,7.9,2.8,8.9,6.9
		c1.4,6-3,11.3-8.6,11.3H325.9z"
                        />
                        <path
                            className={colorNonSelected}
                            d="M358.1,351.1H212c-4.2,0-7.9-2.8-8.9-6.9c-1.4-6,3-11.3,8.6-11.3h146.2c4.2,0,7.9,2.8,8.9,6.9
		C368.1,345.8,363.7,351.1,358.1,351.1z"
                        />
                    </svg>
                </p>
                <h6>Credit Card</h6>
            </div>
        </>
    );
};

export default CreditCardIcon;
