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

const PrepaidIcon = (props) => {
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
                                d="M312.9,170.7v-95c0-20.1-8-39.3-22.2-53.5C276.5,8,257.3,0,237.2,0H104.1C84,0,64.8,8,50.6,22.2
		C36.4,36.4,28.5,55.6,28.4,75.7v360.7c0,20.1,8,39.3,22.2,53.5C64.8,504,84,512,104.1,512h95c7.9,0,14.2-6.4,14.2-14.2
		s-6.4-14.2-14.2-14.2h-95c-12.5,0-24.5-5-33.4-13.8c-8.8-8.9-13.8-20.9-13.8-33.4V75.7c0-12.2,4.7-24,13.2-32.8
		c8.5-8.8,20.1-14,32.3-14.4c0,7.5,3,14.8,8.3,20.1c5.3,5.3,12.6,8.3,20.1,8.3h85.3c7.4,0,14.6-2.9,19.9-8.1
		c5.3-5.2,8.4-12.3,8.6-19.7c11.1,1.8,21.2,7.4,28.5,16c7.3,8.5,11.3,19.4,11.3,30.6v97.4c-43.4,7.3-82.3,31.2-108.6,66.5
		s-37.8,79.5-32.3,123.1c5.5,43.7,27.7,83.5,61.9,111.2c34.2,27.7,77.8,41.1,121.7,37.4c43.9-3.7,84.6-24.2,113.7-57.2
		c29.1-33,44.3-76,42.5-120c-1.8-44-20.6-85.6-52.4-116C399.2,187.7,356.9,170.7,312.9,170.7L312.9,170.7z M312.9,483.6
		c-37.7,0-73.9-15-100.6-41.7c-26.7-26.7-41.7-62.8-41.7-100.6s15-73.9,41.7-100.6c26.7-26.7,62.8-41.7,100.6-41.7
		s73.9,15,100.6,41.7c26.7,26.7,41.7,62.8,41.7,100.6s-15,73.9-41.7,100.6C386.8,468.6,350.6,483.6,312.9,483.6L312.9,483.6z"
                            />
                            <path
                                className={colorSelected}
                                d="M249.5,261.7h21.4c26.2,0,42.2,8.7,46.4,26.2l-67.8,0v15.1h68.7c-2.1,20.8-19.6,34.1-52.1,34.1h-16.6v18.7
		c28.4,30.1,50.2,56.3,74,84.6h28.7c-25.3-30.7-50.6-61.5-74.3-85.9c38.5-1.2,62.9-20.8,65.9-51.2h32.5v-15.3h-32.8
		c-1.3-12.1-7.5-23.2-17.1-30.7H376v-15H249.5L249.5,261.7z"
                            />
                        </g>
                    </svg>
                </p>
                <h6>Mobile Prepaid</h6>
            </div>
        </>
    );
};

export default PrepaidIcon;
