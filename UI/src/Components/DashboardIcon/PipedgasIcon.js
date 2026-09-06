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

const PipedgasIcon = (props) => {
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
                                d="M420,264h-76v-8c0-4.4-3.6-8-8-8h-40v-56.7c33-15.2,56-48.6,56-87.3c0-53-43-96-96-96c-53,0-96,43-96,96
		c0,38.7,23,72.1,56,87.3V248h-40c-4.4,0-8,3.6-8,8v8H92c-50.8,0-92,41.2-92,92.1v140c0,4.4,3.6,8,8,8c4.4,0,8-3.6,8-8v-140
		c0-42,34-76,76-76h76v48h-63.9C81.9,328,64,345.9,64,368v128c0,4.4,3.6,8,8,8s8-3.6,8-8V368c0-13.2,10.8-24,24.1-24H168l0,8
		c0,4.4,3.6,8,8,8h160c4.4,0,8-3.6,8-8v-8H408c13.3,0,24.1,10.7,24.1,24v128c0,4.4,3.6,8,8,8c4.4,0,8-3.6,8-8V368
		c0-22.1-17.9-40-40.1-40H344v-48h76c42,0,76,34,76,76v140c0,4.4,3.6,8,8,8c4.4,0,8-3.6,8-8v-140C512,305.2,470.8,264,420,264z
		 M176,104c0-44.2,35.8-80,80-80c44.2,0,80,35.8,80,80c0,44.2-35.8,80-80,80C211.8,184,176,148.2,176,104z M280,197v51h-48v-51
		c7.8,2,15.8,3,24,3C264.2,200,272.2,199,280,197z M328,344H184v-80h144V344z"
                            />
                            <path
                                className={colorSelected}
                                d="M261.7,98.3c3.1,3.1,3.1,8.2,0,11.3c-3.1,3.1-8.2,3.1-11.3,0l-32-32c-3.1-3.1-3.1-8.2,0-11.3s8.2-3.1,11.3,0
		L261.7,98.3z"
                            />
                        </g>
                    </svg>
                </p>
                <h6>Piped Gas</h6>
            </div>
        </>
    );
};

export default PipedgasIcon;
