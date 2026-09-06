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

const UPITransferIcon = (props) => {
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
                                d="M48,224c4.4,0,8-3.6,8-8v-40h40c4.4,0,8-3.6,8-8c0-4.4-3.6-8-8-8H56c-4.2,0-8.3,1.7-11.3,4.7
		c-3,3-4.7,7.1-4.7,11.3v40C40,220.4,43.6,224,48,224L48,224z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M40,336c0,4.2,1.7,8.3,4.7,11.3c3,3,7.1,4.7,11.3,4.7h40c4.4,0,8-3.6,8-8c0-4.4-3.6-8-8-8H56v-40
		c0-4.4-3.6-8-8-8s-8,3.6-8,8L40,336z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M128,240v-48c0-4.4-3.6-8-8-8H72c-4.4,0-8,3.6-8,8v48c0,4.4,3.6,8,8,8h48C124.4,248,128,244.4,128,240z
		 M112,232H80v-32h32V232z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M88,208h16v16H88V208z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M144,192v48c0,4.4,3.6,8,8,8h48c4.4,0,8-3.6,8-8v-48c0-4.4-3.6-8-8-8h-48C147.6,184,144,187.6,144,192z
		 M160,200h32v32h-32V200z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M168,208h16v16h-16V208z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M144,320c0,4.4,3.6,8,8,8h48c4.4,0,8-3.6,8-8v-48c0-4.4-3.6-8-8-8h-48c-4.4,0-8,3.6-8,8V320z M160,280h32v32
		h-32V280z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M168,288h16v16h-16V288z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M104,272h16v16h-16V272z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M88,288h16v16H88V288z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M104,304h16v16h-16V304z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M72,304h16v16H72V304z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M72,272h16v16H72V272z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M16,436h192v-16H16V92h192V76H16c-4.2,0-8.3,1.7-11.3,4.7C1.7,83.7,0,87.8,0,92v328c0,4.2,1.7,8.3,4.7,11.3
		C7.7,434.3,11.8,436,16,436L16,436z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M208,176v-16h-32c-4.4,0-8,3.6-8,8c0,4.4,3.6,8,8,8H208z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M176,352h32v-16h-32c-4.4,0-8,3.6-8,8S171.6,352,176,352z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M248.2,0c-13.4,0-23.3,10.9-24.2,26.5c0,0.1,0,0.3,0,0.4v464.2c0,0.3,0,0.6,0.1,0.9c1.2,5.6,4.2,10.7,8.6,14.3
		c4.4,3.6,10,5.6,15.7,5.6h231.8c6.3,0,12.4-2.5,16.8-7.1c4.5-4.5,6.9-10.6,6.9-16.9l-0.1-264l0.1,0c4.4,0,8-3.6,8-8v-32
		c0-4.4-3.6-8-8-8c0,0-0.1,0-0.1,0l0-24c0,0,0.1,0,0.1,0c4.4,0,8-3.6,8-8v-32c0-4.4-3.6-8-8-8c0,0-0.1,0-0.1,0v-4c0,0,0.1,0,0.1,0h0
		c4.4,0,8-3.6,8-8V60c0-4.4-3.6-8-8-8c0,0-0.1,0-0.1,0l0-27.7c0-13.7-14.3-24.3-26.6-24.3L248.2,0z M487.8,24.3L488,488
		c0,2.1-0.8,4.2-2.3,5.7c-1.4,1.5-3.4,2.3-5.4,2.3H248.4c-3.6-0.1-6.9-2.2-8.4-5.5V27.2c0.4-6.8,3.6-11.2,8.2-11.2h229
		C481.9,16,487.8,20.7,487.8,24.3L487.8,24.3z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M376.7,44c0,6.6-5.4,12-12,12c-6.6,0-12-5.4-12-12s5.4-12,12-12C371.3,32,376.7,37.4,376.7,44"
                            />
                            <path
                                className={colorNonSelected}
                                d="M392.7,464h-56c-4.4,0-8,3.6-8,8c0,4.4,3.6,8,8,8h56c4.4,0,8-3.6,8-8S397.1,464,392.7,464z"
                            />
                            <path
                                className={colorNonSelected}
                                d="M364,160c-25.5,0-49.9,10.1-67.9,28.1c-18,18-28.1,42.4-28.1,67.9c0,25.5,10.1,49.9,28.1,67.9
		c18,18,42.4,28.1,67.9,28.1s49.9-10.1,67.9-28.1c18-18,28.1-42.4,28.1-67.9c0-25.5-10.2-49.9-28.1-67.8
		C413.8,170.2,389.5,160,364,160z M364,336c-21.2,0-41.6-8.4-56.6-23.4c-15-15-23.4-35.3-23.4-56.6s8.4-41.6,23.4-56.6
		c15-15,35.3-23.4,56.6-23.4s41.6,8.4,56.6,23.4c15,15,23.4,35.3,23.4,56.6c0,21.2-8.5,41.5-23.5,56.5
		C405.5,327.5,385.2,336,364,336z"
                            />
                            <path
                                className={colorSelected}
                                d="M398.3,222.3L348,272.7l-18.3-18.3v0c-3.1-3.1-8.2-3.1-11.3,0c-3.1,3.1-3.1,8.2,0,11.3l24,24
		c1.5,1.5,3.5,2.3,5.7,2.3c2.1,0,4.2-0.8,5.7-2.3l56-56c3.1-3.1,3.1-8.2,0-11.3C406.5,219.2,401.5,219.2,398.3,222.3L398.3,222.3z"
                            />
                        </g>
                    </svg>
                </p>
                <h6>{props.name}</h6>
                {/* <h6>Lead Generation</h6> */}
            </div>
        </>
    );
};

export default UPITransferIcon;
