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

const FRCIcon = (props) => {
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
                            d="M256.3,437c-12.4,0-22.5,10.1-22.5,22.5c0,12.4,10.1,22.5,22.5,22.5c12.4,0,22.5-10.1,22.5-22.5
		C278.8,447.1,268.7,437,256.3,437z M256.3,467c-4.1,0-7.5-3.4-7.5-7.5c0-4.1,3.4-7.5,7.5-7.5c4.1,0,7.5,3.4,7.5,7.5
		C263.8,463.7,260.4,467,256.3,467z"
                        />
                        <path
                            className={colorNonSelected}
                            d="M372.7,0.4L372.7,0.4l-232.7,0c-20.7,0-37.4,16.8-37.4,37.4V474c0,20.7,16.8,37.4,37.4,37.4h232.7
		c20.7,0,37.4-16.8,37.4-37.4V37.9C410.2,17.2,393.4,0.5,372.7,0.4z M394.7,474.4c0,12.5-10.1,22.5-22.5,22.6H139.5
		c-12.4,0-22.5-10.1-22.5-22.6v-52.6h277.7L394.7,474.4z M394.7,407.3H117.1V75.6h277.7V407.3z M117.1,60.2V37.6
		c0-12.5,10.1-22.6,22.5-22.7h232.7c12.4,0,22.5,10.1,22.5,22.7l0,22.7H117.1z"
                        />
                        <path
                            className={colorNonSelected}
                            d="M276.7,30.3l-40.8,0c-3.3,0-6.3,2.1-7.3,5.2c-1.7,5.3,2.3,10.2,7.3,10.2h40.8c3.3,0,6.3-2.1,7.3-5.2
		C285.7,35.2,281.8,30.3,276.7,30.3z"
                        />
                        <path
                            className={colorSelected}
                            d="M319.5,255.8c-0.2-0.2-0.4-0.5-0.6-0.7l-40.5-41l29.1-73.1c2.7-6.9-0.6-14.8-7.6-17.5c-5-2-10.7-0.8-14.5,2.9
		l-91.7,90.8c-5.3,5.2-5.3,13.8-0.1,19.1c0.2,0.2,0.5,0.5,0.7,0.7l0,0l36.3,31.6l-32.3,75.7c-2.5,5.8-0.7,12.5,4.4,16.2
		c5,3.8,12,3.5,16.8-0.7l98.5-85C323.8,269.9,324.4,261.4,319.5,255.8z M214.7,344.2l30.2-70.8c2.3-5.4,0.9-11.6-3.5-15.5
		l-35.9-31.3l85.5-84.6l-26.8,67.4c-2,5-0.8,10.6,2.9,14.5l40,40.6L214.7,344.2z"
                        />
                    </svg>
                </p>
                <h6>FRC</h6>
            </div>
        </>
    );
};

export default FRCIcon;
