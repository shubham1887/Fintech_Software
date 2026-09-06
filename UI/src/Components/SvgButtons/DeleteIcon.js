import React from 'react';

import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';

const useStyles = makeStyles({
    ColorFill: {
        fill: '#ff0000'
    }
});

const DeleteIcon = (props) => {
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
                viewBox="0 0 172.4 60.6"
            >
                <g>
                    <rect
                        x="137.1"
                        y="27.6"
                        className={classesInline.ColorFill}
                        width="1.6"
                        height="9.5"
                    />
                    <path
                        className={classesInline.ColorFill}
                        d="M141.8,21.9c0-0.7-0.6-1.3-1.3-1.3h-5.2c-0.7,0-1.3,0.6-1.3,1.3v1.7h-5.6v1.6h1.7l1.3,15.1
		c0,0.4,0.4,0.7,0.8,0.7h11.5c0.4,0,0.8-0.3,0.8-0.7l1.3-15.1h1.7v-1.6h-5.6V21.9z M135.5,22.2h4.7v1.5h-4.7V22.2z M144.1,25.3
		l-1.3,14.2h-10l-1.3-14.2H144.1z"
                    />

                    <rect
                        x="134.1"
                        y="27.6"
                        transform="matrix(0.9993 -3.659694e-02 3.659694e-02 0.9993 -1.0946 4.9597)"
                        className={classesInline.ColorFill}
                        width="1.6"
                        height="9.5"
                    />

                    <rect
                        x="136"
                        y="31.6"
                        transform="matrix(3.663218e-02 -0.9993 0.9993 3.663218e-02 103.2757 171.8917)"
                        className={classesInline.ColorFill}
                        width="9.5"
                        height="1.6"
                    />
                    <path
                        className={classesInline.ColorFill}
                        d="M45.4,27.3c-2.1,0-3.7,1.3-4.1,3.7h8.2C49.5,28.8,47.7,27.3,45.4,27.3z"
                    />
                    <path
                        className={classesInline.ColorFill}
                        d="M24.6,23.2h-3.1v14.3h3.1c4.6,0,7.1-2.6,7.1-7.1S29.2,23.2,24.6,23.2z"
                    />
                    <path
                        className={classesInline.ColorFill}
                        d="M70.4,27.3c-2.1,0-3.7,1.3-4.1,3.7h8.2C74.4,28.8,72.6,27.3,70.4,27.3z"
                    />
                    <path
                        className={classesInline.ColorFill}
                        d="M171,0H1.4C0.6,0,0,0.6,0,1.4v57.7c0,0.8,0.6,1.4,1.4,1.4H171c0.8,0,1.4-0.6,1.4-1.4V1.4
		C172.4,0.6,171.7,0,171,0z M24.8,41.2h-7.6V19.6h7.6c6.9,0,11.4,4.3,11.4,10.9C36.2,37,31.7,41.2,24.8,41.2z M53.9,34H41.3
		c0.2,2.5,2,4,4.1,4c1.9,0,2.9-0.9,3.4-2.1h4.7c-0.9,3.2-3.8,5.7-8,5.7c-5,0-8.6-3.5-8.6-8.9c0-5.4,3.5-8.9,8.6-8.9
		c5,0,8.5,3.4,8.5,8.5C54,32.8,54,33.4,53.9,34z M60.1,41.2h-4.3v-23h4.3V41.2z M78.9,34H66.3c0.2,2.5,2,4,4.1,4
		c1.9,0,2.9-0.9,3.4-2.1h4.7c-0.9,3.2-3.8,5.7-8,5.7c-5,0-8.6-3.5-8.6-8.9c0-5.4,3.5-8.9,8.6-8.9c5,0,8.5,3.4,8.5,8.5
		C79,32.8,79,33.4,78.9,34z M89.7,27.6h-3.8v8.3c0,1.1,0.5,1.6,1.8,1.6h2v3.7H87c-3.3,0-5.5-1.4-5.5-5.3v-8.3h-2v-3.6h2v-4.3h4.4
		v4.3h3.8V27.6z M107.3,34H94.7c0.2,2.5,2,4,4.1,4c1.9,0,2.9-0.9,3.4-2.1h4.7c-0.9,3.2-3.8,5.7-8,5.7c-5,0-8.6-3.5-8.6-8.9
		c0-5.4,3.5-8.9,8.6-8.9c5,0,8.5,3.4,8.5,8.5C107.4,32.8,107.4,33.4,107.3,34z M137.4,49.2c-10.4,0-18.9-8.5-18.9-18.9
		c0-10.4,8.5-18.9,18.9-18.9s18.9,8.5,18.9,18.9C156.3,40.7,147.8,49.2,137.4,49.2z"
                    />
                    <path
                        className={classesInline.ColorFill}
                        d="M98.8,27.3c-2.1,0-3.7,1.3-4.1,3.7h8.2C102.8,28.8,101,27.3,98.8,27.3z"
                    />
                </g>
            </svg>
        </>
    );
};

export default DeleteIcon;
