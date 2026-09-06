import React, { useEffect, useState } from 'react';
import { Button } from '@material-ui/core';
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

const UpiIcon = (props) => {
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
                        <g id="g126" transform="translate(316.4629,76.4951)">
                            <path
                                id="path128"
                                className={colorNonSelected}
                                d="M146.5,253.8h-31.8l44.3-160h31.8L146.5,253.8z"
                            />
                        </g>
                        <g id="g130" transform="translate(306.4521,170.2881)">
                            <path
                                id="path132"
                                className={colorSelected}
                                d="M139.9,5.2c-2.2-3-5.6-4.6-10.2-4.6H-45.4l-8.7,31.3h31.9v0h127.4L96,65.4H-31.5l0-0.1h-31.9
		l-26.4,95.4h31.9l17.7-64.1h143.3c4.5,0,8.7-1.5,12.6-4.6c4-3.1,6.6-6.8,7.8-11.3l17.7-64.1C142.6,12.1,142.1,8.2,139.9,5.2"
                            />
                        </g>
                        <g id="g134" transform="translate(156.1196,82.541)">
                            <path
                                id="path136"
                                className={colorNonSelected}
                                d="M42,237.8c-1.8,6.3-7.5,10.7-14.1,10.7h-164.3c-4.5,0-7.8-1.5-10-4.6c-2.2-3.1-2.7-6.8-1.4-11.3
		l40.1-144.3h31.9l-35.8,128.9H15.9L51.7,88.3h31.9L42,237.8z"
                            />
                        </g>
                    </svg>
                </p>
                <h6>UPI</h6>
            </div>
        </>
    );
};

export default UpiIcon;
