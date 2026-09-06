import React from 'react';

import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';

const useStyles = makeStyles({
    ColorFill: {
        fill: '#fff'
    }
});

const PlanIcon = (props) => {
    const propsInline = { hoverColor: PRIMARY_COLOR() };

    const classesInline = useStyles(propsInline);

    return (
        <>
            <svg
                version="1.1"
                id="Layer_1"
                xmlns="http://www.w3.org/2000/svg"
                x="0px"
                y="0px"
                viewBox="0 0 37.8 37.8"
                style={{
                    width: '32px'
                }}
            >
                <g>
                    <path
                        className={classesInline.ColorFill}
                        d="M26,11h-14v0.5h1v2.1h-1V15h1V17h-1v1.1h1v2h-1v1.4h1v2h-1V25h1v2h-1v0.7h14V11z M16,26.5l-2.3-2.3l1.4-1.4
		l0.9,0.9l1.9-1.9l1.4,1.4L16,26.5z M16,21.7l-2.3-2.3l1.4-1.4l0.9,0.9l1.9-1.9l1.4,1.4L16,21.7z M16,16.8l-2.3-2.3l1.4-1.4L16,14
		l1.9-1.9l1.4,1.4L16,16.8z M24.7,25.6h-4.6v-2h4.6V25.6z M24.7,20.4h-4.6v-2h4.6V20.4z M24.7,15.6h-4.6v-2.1h4.6V15.6z"
                    />
                    <path
                        className={classesInline.ColorFill}
                        d="M18.9,0C8.5,0,0,8.5,0,18.9c0,10.4,8.5,18.9,18.9,18.9c10.4,0,18.9-8.5,18.9-18.9C37.8,8.5,29.3,0,18.9,0z
		 M28,29.6H10V27H9v-1.8h1v-1.4H9v-2h1v-1.4H9v-2h1V17H9v-2h1v-1.3H9v-2.1h1V9.1h18V29.6z"
                    />
                </g>
            </svg>
        </>
    );
};

export default PlanIcon;
