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

const CmsIcon = (props) => {
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
                            enableBackground: 'new 0 0 503.2 512',
                            width: '36px'
                        }}
                    >
                        <g>
                            <path
                                class={colorNonSelected}
                                d="M482.7,0H20.5C9.2,0,0,9.3,0,20.7v470.7C0,502.7,9.2,512,20.5,512h462.2c11.3,0,20.5-9.2,20.5-20.7V20.7
		C503.2,9.3,494,0,482.7,0z M485.5,493.6c-0.3,0.3-0.4,0.4-0.7,0.7H18.4c-0.3-0.3-0.4-0.4-0.7-0.7V114.8h467.9V493.6z M485.5,88.3
		H17.7V18.4c0.3-0.3,0.4-0.4,0.7-0.7h466.4c0.3,0.3,0.4,0.4,0.7,0.7V88.3z"
                            />
                            <g>
                                <path
                                    class={colorSelected}
                                    d="M388.4,267.8c-3.2-11.9-7.9-23.3-14.1-33.9L387,209l-39.8-39.8l-24.8,12.7c-10.7-6.2-22.1-10.9-34-14.1
			l-8.6-26.5h-56.3l-8.6,26.5c-11.9,3.2-23.3,7.9-33.9,14.1L156,169.1L116.2,209l12.7,24.9c-6.2,10.6-10.9,22.1-14.1,33.9l-26.5,8.6
			v56.3l26.5,8.6c3.2,11.9,8,23.3,14.1,33.9l-12.7,24.8l39.8,39.8l24.9-12.7c10.6,6.2,22.1,10.9,33.9,14.1l8.6,26.5h56.3l8.6-26.5
			c11.9-3.2,23.3-7.9,33.9-14.1l24.9,12.6l39.8-39.8l-12.7-24.9c6.1-10.6,10.9-22,14-33.9l26.5-8.6v-56.3L388.4,267.8z M397.2,318.5
			l-23.8,7.7l-1.4,6c-3,13.5-8.4,26.4-15.8,38.1l-3.3,5.1l11.4,22.3l-19.7,19.7L322.4,406l-5.1,3.3c-11.7,7.4-24.7,12.8-38.2,15.9
			l-5.8,1.3l-7.7,23.8h-27.9l-7.7-23.8l-5.9-1.3c-13.5-3-26.3-8.3-38-15.7l-5.3-3.4l-22.3,11.4l-19.7-19.7l11.4-22.3l-3.3-5.2
			c-7.4-11.7-12.7-24.6-15.8-38.1l-1.3-5.9l-23.8-7.7v-27.9l23.8-7.7l1.4-6.1c3-13.5,8.4-26.3,15.8-38l3.3-5.1l-11.4-22.3l19.7-19.7
			l22.3,11.4l5.2-3.3c11.7-7.4,24.6-12.7,38.1-15.8l5.9-1.3l7.7-23.8h27.9l7.7,23.8l6,1.4c13.5,3.1,26.4,8.4,38.1,15.8l5.1,3.2
			l22.3-11.4l19.7,19.7l-11.4,22.3l3.3,5.1c7.4,11.8,12.8,24.7,15.8,38.3l1.3,5.8l23.8,7.7L397.2,318.5z"
                                />
                                <path
                                    class={colorSelected}
                                    d="M252.8,218.1c-48.4,0-87.7,39.2-87.7,87.7s39.2,87.7,87.7,87.7s87.7-39.2,87.7-87.7c0,0,0,0,0,0
			C340.4,257.3,301.2,218.1,252.8,218.1z M320.6,305.7c0,37.4-30.4,67.7-67.8,67.8c-37.4,0-67.8-30.3-67.8-67.8
			c0-37.4,30.3-67.8,67.8-67.8C290.2,237.9,320.6,268.3,320.6,305.7C320.6,305.7,320.6,305.7,320.6,305.7z"
                                />
                            </g>
                            <path
                                class={colorNonSelected}
                                d="M200.3,57c0-5.5,4.4-9.9,9.9-12.9h202.6c5.5,3.1,9.8,7.7,9.6,13.2c-0.2,5.3-4.4,9.5-9.6,4.5H210.2
		C204.7,66.9,200.3,62.5,200.3,57z"
                            />
                        </g>
                    </svg>
                </p>
                <h6>{props.name}</h6>
            </div>
        </>
    );
};

export default CmsIcon;
