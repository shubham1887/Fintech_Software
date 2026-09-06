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

const WalletTopUpIcon = (props) => {
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
                            d="M370,512H142c-25.4,0-46-20.6-46-46V46c0-25.4,20.6-46,46-46h228c25.4,0,46,20.6,46,46v420
	C416,491.4,395.4,512,370,512z M142,32c-7.7,0-14,6.3-14,14v420c0,7.7,6.3,14,14,14h228c7.7,0,14-6.3,14-14V46c0-7.7-6.3-14-14-14
	H142z"
                        />
                        <path
                            className={colorNonSelected}
                            d="M288,88h-64c-8.8,0-16-7.2-16-16s7.2-16,16-16h64c8.8,0,16,7.2,16,16S296.8,88,288,88z"
                        />
                        <circle
                            className={colorSelected}
                            cx="256"
                            cy="420.9"
                            r="27"
                        />
                    </svg>
                </p>
                <h6>Wallet TopUp</h6>
            </div>
        </>
    );
};

export default WalletTopUpIcon;
