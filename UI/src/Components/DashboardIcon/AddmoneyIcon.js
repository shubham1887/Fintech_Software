import React, { useState } from 'react';
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

const AddmoneyIcon = (props) => {
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
                                d="M299.9,206.6c0-6.8-5.5-12.3-12.3-12.3H138.1c-6.8,0-12.3,5.5-12.3,12.3c0,6.8,5.5,12.3,12.3,12.3h60.5
		c17.7,0,32.7,12,37.3,28.3h-97.8c-6.8,0-12.3,5.5-12.3,12.3c0,6.8,5.5,12.3,12.3,12.3h96.5c-5.7,14.3-19.7,24.5-36,24.5h-60.5
		c-5,0-9.6,3.1-11.4,7.7c-1.9,4.7-0.7,10,3,13.5l100.3,95.2c2.4,2.3,5.4,3.4,8.5,3.4c3.3,0,6.5-1.3,8.9-3.8
		c4.7-4.9,4.5-12.7-0.4-17.4L169,321.1h29.7c30.1,0,55.2-21,61.7-49.2h27.2c6.8,0,12.3-5.5,12.3-12.3c0-6.8-5.5-12.3-12.3-12.3
		h-26.5c-1.8-10.5-6.1-20.2-12.4-28.3h38.9C294.4,218.9,299.9,213.4,299.9,206.6L299.9,206.6z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M422.4,185.1c-6.8,0-12.3,5.5-12.3,12.3v240.3c0,27.2-22.1,49.3-49.3,49.3H77.3C50.1,487.1,28,465,28,437.8
		V142c0-27.2,22.1-49.3,49.3-49.3h221.8c6.8,0,12.3-5.5,12.3-12.3c0-6.8-5.5-12.3-12.3-12.3H77.3c-40.8,0-73.9,33.2-73.9,73.9v295.8
		c0,40.8,33.2,73.9,73.9,73.9h283.4c40.8,0,73.9-33.2,73.9-73.9V197.5C434.7,190.7,429.2,185.1,422.4,185.1L422.4,185.1z"
                            />
                            <path
                                className={colorSelected}
                                d="M496.3,68.1h-61.6V12.6c0-6.8-5.5-12.3-12.3-12.3c-6.8,0-12.3,5.5-12.3,12.3v55.5h-60.9
		c-6.8,0-12.3,5.5-12.3,12.3c0,6.8,5.5,12.3,12.3,12.3l60.9,0v55.5c0,6.8,5.5,12.3,12.3,12.3c6.8,0,12.3-5.5,12.3-12.3V92.7h61.6
		c6.8,0,12.3-5.5,12.3-12.3C508.6,73.6,503.1,68.1,496.3,68.1L496.3,68.1z"
                            />
                        </g>
                    </svg>
                </p>
                <h6>Add Money</h6>
            </div>
        </>
    );
};

export default AddmoneyIcon;
