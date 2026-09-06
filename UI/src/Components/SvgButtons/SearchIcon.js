import React from 'react';

import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';

const useStyles = makeStyles({
    ColorFill: {
        fill: '#fff'
    }
});

const SearchIcon = (props) => {
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
                        d="M16.3,11.4c-2.7,0-5,2.2-5,5c0,2.7,2.2,5,5,5c0,0,0,0,0,0c2.7,0,5-2.2,5-5C21.3,13.6,19,11.4,16.3,11.4z"
                    />
                    <path
                        className={classesInline.ColorFill}
                        d="M18.9,0C8.5,0,0,8.5,0,18.9c0,10.4,8.5,18.9,18.9,18.9s18.9-8.5,18.9-18.9C37.8,8.5,29.3,0,18.9,0z M28.2,28.7
		c-0.6,0.6-1.7,0.6-2.3-0.1L20.2,23c-2.6,1.5-5.9,1.4-8.4-0.3c-3.5-2.5-4.3-7.3-1.8-10.7c2.5-3.5,7.3-4.3,10.7-1.8
		c3.5,2.5,4.3,7.3,1.8,10.7l5.6,5.6c0,0,0,0,0.1,0.1C28.9,27.1,28.8,28.1,28.2,28.7z"
                    />
                </g>
            </svg>
        </>
    );
};

export default SearchIcon;
