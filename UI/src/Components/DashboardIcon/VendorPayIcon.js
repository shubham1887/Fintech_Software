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

const VendorPayIcon = (props) => {
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
                                d="M163.9,164.9c0,50.8,41.3,92.1,92.1,92.1c50.8,0,92.1-41.3,92.1-92.1c0-50.8-41.3-92.1-92.1-92.1
		C205.2,72.8,163.9,114.2,163.9,164.9L163.9,164.9z M331.6,164.9c0,41.7-33.9,75.6-75.6,75.6c-41.7,0-75.6-33.9-75.6-75.6
		c0-41.7,33.9-75.6,75.6-75.6C297.7,89.4,331.6,123.3,331.6,164.9L331.6,164.9z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M19.9,439.2h50.2c13.6,0,19.9-6.3,19.9-19.9v-8l130.2,24.3c9.9,1.6,19.2,2.4,28.2,2.4c22.3,0,42.5-5,65.1-15.6
		l181.8-85.5c15.5-7.3,20.2-18.8,14.1-34.2l-0.3-0.7c-7.5-19-21.7-18.3-33.2-15.7L355,314c-1.7-6.7-5.5-12.6-11.2-17.1
		c-7.2-5.7-17-8.5-27.5-7.8l-38.8,2.3c-26.3,1.6-42.9-2.7-58.9-6.9c-13.7-3.6-26.7-7-43.7-5.9c-27.3,1.6-61.7,14.8-84.7,25.1v-14.9
		c0-13.6-6.3-19.9-19.9-19.9H19.9C6.3,269,0,275.3,0,288.9v130.4C0,432.8,6.3,439.2,19.9,439.2L19.9,439.2z M90.1,321.9
		c21.6-10,58.4-25.2,85.7-26.8c14.4-0.9,25.6,2,38.6,5.4c16.5,4.3,35.1,9.1,64.1,7.4l38.8-2.3c6.5-0.4,12.3,1.2,16.3,4.4
		c3.5,2.8,5.6,6.6,5.8,11c0.3,4.2-1.3,8.3-4.5,11.5c-3.7,3.7-9.2,5.9-15.7,6.3l-31.8,1.9c-50.2,3-76.2,16.5-77.3,17.1
		c-4,2.1-5.6,7.1-3.4,11.1c1.5,2.8,4.3,4.4,7.3,4.4c1.3,0,2.6-0.3,3.8-0.9c0.2-0.1,24.4-12.4,70.6-15.2l31.8-1.9
		c10.5-0.6,19.8-4.6,26.4-11.1c3.8-3.8,6.4-8.2,7.9-13.1l125.1-28.7c10.5-2.4,11.8-0.1,14.1,5.6l0.3,0.7c2.7,6.9,2.3,9.4-5.8,13.2
		l-181.8,85.5c-28.8,13.5-52.2,16.9-83.4,11.9l-133-24.8L90.1,321.9z M16.7,285.7h56.6v136.7l-56.6,0L16.7,285.7z"
                            />
                            <path
                                className={colorSelected}
                                d="M275.8,146.4h-8.3c-0.4-1.5-1.1-3.1-2.2-4.4h10.5c1.8,0,3.3-1.5,3.3-3.3c0-1.8-1.5-3.3-3.3-3.3l-26.8,0h-12.7
		c-1.8,0-3.3,1.5-3.3,3.3c0,1.8,1.5,3.3,3.3,3.3H249c6.8,0,9.9,1.8,11.4,4.4l-24.2,0c-1.8,0-3.3,1.5-3.3,3.3c0,1.8,1.5,3.3,3.3,3.3
		h25.2c-0.2,4.6-2.2,8.8-12.5,8.8l-12.7,0c-1.3,0-2.6,0.9-3.1,2c-0.4,1.3-0.2,2.6,0.7,3.5l26.3,26.3c0.7,0.7,1.5,0.9,2.4,0.9
		s1.8-0.2,2.4-0.9c1.3-1.3,1.3-3.3,0-4.6l-20.6-20.6h4.6c15.8,0,18.9-9.2,19.1-15.4h7.7c1.8,0,3.3-1.5,3.3-3.3
		C279.1,147.9,277.5,146.4,275.8,146.4L275.8,146.4z"
                            />
                        </g>
                    </svg>
                </p>
                <h6>{props.name}</h6>
            </div>
        </>
    );
};

export default VendorPayIcon;
