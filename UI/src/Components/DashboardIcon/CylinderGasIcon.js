import React, { useEffect, useState } from 'react';
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

const CylinderGasIcon = (props) => {
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
                            d="M333.7,103.1V51.2h8.5c9.1,0,17.6-4.9,22.2-12.8c4.6-7.9,4.6-17.7,0-25.6C359.8,4.9,351.4,0,342.2,0H103.3
	c-9.1,0-17.6,4.9-22.2,12.8c-4.6,7.9-4.6,17.7,0,25.6c4.6,7.9,13,12.8,22.2,12.8h8.5v51.9c-14.2,2-27.2,9.1-36.6,20
	c-9.4,10.8-14.6,24.7-14.6,39.1v247.5c0,14.4,5.2,28.2,14.6,39.1c9.4,10.8,22.4,17.9,36.6,20v34.8c0,2.3,0.9,4.4,2.5,6
	s3.8,2.5,6,2.5h204.8c2.3,0,4.4-0.9,6-2.5s2.5-3.8,2.5-6v-34.8c14.2-2,27.2-9.1,36.6-20c9.4-10.8,14.6-24.7,14.6-39.1V162.1
	c0-14.4-5.2-28.2-14.6-39.1C360.9,112.2,347.9,105.1,333.7,103.1L333.7,103.1z M103.3,34.1c-4.7,0-8.5-3.8-8.5-8.5s3.8-8.5,8.5-8.5
	h238.9c4.7,0,8.5,3.8,8.5,8.5s-3.8,8.5-8.5,8.5H103.3z M316.6,51.2v51.2h-17.1V51.2H316.6z M239.8,68.3h-34.1
	c-6.8,0-13.3,2.7-18.1,7.5c-4.8,4.8-7.5,11.3-7.5,18.1v8.5H163V51.2h119.5v51.2h-17.1v-8.5c0-6.8-2.7-13.3-7.5-18.1
	C253.1,71,246.6,68.3,239.8,68.3L239.8,68.3z M248.4,93.9v8.5h-51.2v-8.5c0-4.7,3.8-8.5,8.5-8.5h34.1c2.3,0,4.4,0.9,6,2.5
	S248.4,91.6,248.4,93.9z M146,51.2v51.2h-17.1V51.2H146z M316.6,494.9H128.9v-25.6h187.7V494.9z M367.8,409.6
	c0,11.3-4.5,22.2-12.5,30.2c-8,8-18.9,12.5-30.2,12.5H120.4c-11.3,0-22.2-4.5-30.2-12.5c-8-8-12.5-18.9-12.5-30.2V162.1
	c0-11.3,4.5-22.2,12.5-30.2c8-8,18.9-12.5,30.2-12.5h204.8c11.3,0,22.2,4.5,30.2,12.5c8,8,12.5,18.9,12.5,30.2V409.6z"
                        />
                        <path
                            className={colorSelected}
                            d="M298.1,251.2c-1.2-1.9-3.2-3.2-5.4-3.6c-2.2-0.4-4.5,0-6.4,1.3l-8.5,5.6c3-18.4-2.8-37.2-15.7-50.8
	c-10.6-11.3-22-21.7-34.1-31.3c-3.4-2.6-8.2-2.3-11.2,0.7c-4,3.9-96.4,96.3-96.4,151.1c0,26.1,10,51.2,27.9,70.1
	c17.9,19,42.4,30.4,68.4,31.9c4,0.3,8.1,0.3,12.1,0c26-1.5,50.5-13,68.4-31.9c17.9-19,27.9-44,27.9-70.1
	C325.2,304.1,314.9,276.1,298.1,251.2L298.1,251.2z M222.8,409.6c-11.3,0-22.2-4.5-30.2-12.5c-8-8-12.5-18.9-12.5-30.2
	c0.1-7.3,1.3-14.6,3.6-21.5l17.3,11.5c2.9,2,6.7,1.9,9.6-0.1c2.9-2,4.2-5.5,3.5-8.9l-3-13.1c-2.9-12.9,0.3-26.3,8.6-36.5
	c1.1-1.4,2.1-2.6,3.1-3.8c18.7,22.1,42.7,56.1,42.7,72.3c0,11.3-4.5,22.2-12.5,30.2C244.9,405.1,234.1,409.6,222.8,409.6
	L222.8,409.6z M278.2,388.9c2.8-7,4.3-14.5,4.3-22c0-30.6-48-84.9-53.4-91.1c-1.6-1.8-3.9-2.8-6.3-2.8c-2.4,0-4.7,1-6.3,2.8
	c0,0-4.1,4.5-9.9,11.7c-10,12.3-14.8,27.9-13.3,43.7l-8.4-5.5c-1.9-1.4-4.4-1.8-6.7-1.2c-2.3,0.5-4.3,2-5.5,4
	c-6.4,11.8-9.7,25-9.6,38.4c0,7.5,1.5,15,4.3,22c-19-16.1-29.9-39.8-29.9-64.7c0-37,57.8-104.4,85.9-133.5
	c9.3,7.8,18.1,16,26.4,24.8c9.6,10.1,13.7,24.2,10.9,37.8l-3.6,18c-0.7,3.4,0.7,6.9,3.6,8.8c2.8,2,6.6,2,9.5,0l18.3-12.2
	c11.1,16.9,17.9,36.2,19.6,56.3C308.1,349.2,297.2,372.8,278.2,388.9L278.2,388.9z"
                        />
                    </svg>
                </p>
                <h6>LPG Gas</h6>
            </div>
        </>
    );
};

export default CylinderGasIcon;
