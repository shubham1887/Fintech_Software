import React, { useEffect, useState } from 'react';
import { Button } from '@material-ui/core';
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

const DthIcon = (props) => {
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
                    {/* <svg
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
                            d="M437,75C388.7,26.6,324.4,0,256,0S123.3,26.6,75,75C26.6,123.3,0,187.6,0,256s26.6,132.7,75,181
	c48.4,48.4,112.6,75,181,75s132.7-26.6,181-75c48.4-48.4,75-112.6,75-181S485.4,123.3,437,75L437,75z M415.8,415.8
	C373.1,458.5,316.4,482,256,482s-117.1-23.5-159.8-66.2S30,316.4,30,256S53.5,138.9,96.2,96.2S195.6,30,256,30
	s117.1,23.5,159.8,66.2S482,195.6,482,256S458.5,373.1,415.8,415.8z"
                        />
                        <g>
                            <path
                                className={colorSelected}
                                d="M349,142.3H175.5v34.5h61.2c22.5,0,41.6,14.4,48.7,34.5H175.5v34.5h109.9c-7.1,20.1-26.3,34.5-48.7,34.5h-61.2
		v34.5l101,101l24.4-24.4l-76.6-76.6h12.4c41.6,0,76.4-29.6,84.4-68.9H349v-34.5h-27.9c-2.6-12.7-8-24.4-15.5-34.5H349V142.3z"
                            />
                        </g>
                    </svg> */}
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
                                d="M508.1,131.3l-81.4-76c-2.3-2.2-5.4-3.3-8.5-3.1l0,0c-6.2,0.3-11.1,5.4-11.1,11.7v29.2H290.9
		c-6.4,0-11.6,5.2-11.6,11.6v20.9c-7.7-1.4-15.2-3.5-23.3-3.5c-73.8,0-133.8,60-133.8,133.8c0,25.6,7.3,49.5,19.8,69.8h-37.3v-28.5
		c0-4.7-2.7-8.9-7-10.7c-4.3-1.9-9.2-1-12.6,2.2L3.9,364.2h0c-2.4,2.2-3.7,5.3-3.7,8.5c0,3.2,1.3,6.3,3.7,8.5l81.2,75.4h0
		c3.4,3.2,8.3,4.1,12.6,2.2c4.3-1.9,7-6.1,7-10.7v-28.9h116.2c6.4,0,11.6-5.2,11.6-11.6v-20.1c7.7,1.4,15.2,3.5,23.3,3.5
		c73.8,0,133.8-60.3,133.8-134.4c0-25.7-7.3-49.7-19.8-70.1H407v29.2c0,4.7,2.7,8.9,7,10.8c4.3,1.9,9.3,1,12.6-2.2l81.4-76
		c2.4-2.2,3.7-5.3,3.7-8.6C511.8,136.6,510.4,133.5,508.1,131.3z M430.9,189.2v-13.8c0-6.4-5.2-11.6-11.6-11.6h-72.1
		c-4.6,0-8.8,2.7-10.7,6.9c-1.9,4.2-1.1,9.2,2,12.6c18.5,20.6,29.4,47.7,28.3,77.8c-2.2,58.7-50.7,105.8-109.4,106.3
		c-11.4,0.1-22.4-1.5-32.8-4.5c-3.5-1-7.3-0.3-10.2,1.8c-2.9,2.2-4.7,5.6-4.7,9.3v22.5H93.3v0c-6.3,0-11.4,5.2-11.4,11.7v14.1
		L30.2,373l51.6-49.1V338c0,6.5,5.1,11.7,11.4,11.7h72.6c4.6,0,8.8-2.7,10.7-6.9c1.9-4.2,1.1-9.2-2-12.6
		c-16.6-18.6-27.2-42.5-28.2-69.2c-2.6-65.2,52.5-119,117.7-114.7c8.3,0.5,16.4,2,24.2,4.3c3.5,1,7.3,0.3,10.2-1.8
		c2.9-2.2,4.7-5.6,4.7-9.3V117h116.1c6.4,0,11.6-5.2,11.6-11.6v-14l52.5,48.9L430.9,189.2z"
                            />
                            <path
                                className={colorSelected}
                                d="M215.5,217.8h34.6c5.1,0,9.4,2.9,11.8,7h-46.4v18.2h46.4c-2.8,4.5-7.1,7-11.8,7h-44.8l67.8,72.2l12.7-13.5
		l-38.1-40.5h1.6c14.8-0.4,27.3-10.8,31.3-25.3h20.5v-17.5h-19.6c-0.8-2.8-1.6-5.2-2.4-7h22.6l0-18.9h-86.3L215.5,217.8z"
                            />
                        </g>
                    </svg>
                </p>
                <h6>{props.name}</h6>
            </div>
        </>
    );
};

export default DthIcon;
