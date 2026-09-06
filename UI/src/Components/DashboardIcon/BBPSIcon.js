import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';
import React, { useState } from 'react';

const useStyles = makeStyles({
    dashboardserviceicons: (props) => ({
        padding: '15px',
        borderRadius: '3px',
        '&:hover': {
            background: `${props.hoverColor} !important`,
            color: '#fff'
        }
    })
});
const BBPSIcon = (props) => {
    const propsInline = { hoverColor: PRIMARY_COLOR() };

    const classesInline = useStyles(propsInline);
    return (
        <>
            <div className={classesInline.dashboardserviceicons}>
                <svg
                    version="1.1"
                    id="Layer_1"
                    xmlns="http://www.w3.org/2000/svg"
                    xmlnsXlink="http://www.w3.org/1999/xlink"
                    x="0px"
                    y="0px"
                    viewBox="0 0 380.9 454"
                    style={{
                        enableBackground: 'new 0 0 380.9 454',
                        marginTop: '10px',
                        marginBottom: '15px',
                        width: '20%'
                    }}
                >
                    <style>
                        {`
          .st0 { fill: #A9A8A9; }
          .st1 { fill: #FDB932; }
          .st2 { fill: #F47A20; }
        `}
                    </style>
                    <g>
                        <rect
                            x="828"
                            y="-52"
                            class="st0"
                            width="353.5"
                            height="353.5"
                        />
                    </g>
                    <g>
                        <g id="g70_31_" transform="translate(56.6401,204.5239)">
                            <path
                                id="path72_31_"
                                class="st1"
                                d="M58.6-126.2h80.2c9,0,17.7,0.8,26,2.3c8.4,1.5,15.7,4.2,22.2,8c6.4,3.9,11.6,9.2,15.4,16.1
			c3.8,6.8,5.8,15.6,5.8,26.3c0,19.3-5.8,33.2-17.3,41.8c29.7,5.7,51.8,11.9,75.2,23.1c9.8-7.2,17.8-15.3,24.1-24.4
			c10.5-15.4,15.7-34.7,15.7-57.8c0-21.4-3.5-39.4-10.6-53.9c-7.1-14.5-17-26.2-29.9-35c-12.8-8.8-28.3-15.1-46.2-18.9
			c-18-3.8-37.9-5.8-59.7-5.8H58.6V-126.2z"
                            />
                        </g>
                        <g
                            id="g74_31_"
                            transform="translate(108.8408,117.2216)"
                        >
                            <path
                                id="path76_31_"
                                class="st2"
                                d="M166.9,230.2c-4,7.6-9.4,13.7-16.2,18.1c-6.8,4.5-14.6,7.6-23.5,9.5
			c-8.9,1.9-18.2,2.8-27.9,2.8H-7.4V136.3h108.6c21.6,0,38.9,5,52.1,14.9c13.1,9.9,19.7,26.6,19.7,49.9
			C172.9,212.9,170.9,222.6,166.9,230.2 M-7.4,64.8V-38.9v-78.4h-101.4v454h220.7c20.3,0,40.1-2.5,59.4-7.6
			c19.3-5.1,36.4-12.9,51.5-23.5c15-10.6,27-24.3,35.9-41c8.9-16.7,13.3-36.6,13.3-59.5c0-28.4-7.4-52.3-20.7-72.8
			c-25.6-39.6-81-65.8-158.6-65.8"
                            />
                        </g>
                    </g>
                </svg>
                <h6>{props.name}</h6>
            </div>
        </>
    );
};

export default BBPSIcon;
