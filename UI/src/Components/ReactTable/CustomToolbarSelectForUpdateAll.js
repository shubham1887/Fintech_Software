import React from 'react';
import IconButton from '@material-ui/core/IconButton';
import Tooltip from '@material-ui/core/Tooltip';
import DoneAllIcon from '@material-ui/icons/DoneAll';
import { makeStyles } from '@material-ui/core/styles';

const useStyles = makeStyles((theme) => ({
    iconContainer: {
        marginRight: '24px'
    }
}));

const CustomToolbarSelectForUpdateAll = ({
    selectedRows,
    displayData,
    setSelectedRows,
    primaryIndex,
    onUpdateCallBack,
    title
}) => {
    const classes = useStyles();

    const successAllClicked = async () => {
        var array = [];

        displayData.map((item) => {
            //array.push({ ID: displayData[item.index].data[primaryIndex] });
        });

        selectedRows.data.map((item) => {
            //array.push({ ID: displayData[item.index].data[primaryIndex] });
        });

        //var request = JSON.stringify(displayData);
        //onUpdateCallBack(request);
    };

    return (
        <div className={classes.iconContainer}>
            <Tooltip title={title}>
                <IconButton
                    className={classes.iconButton}
                    onClick={successAllClicked}
                >
                    <DoneAllIcon style={{ fill: 'green' }} />
                    {title}
                </IconButton>
            </Tooltip>
        </div>
    );
};

export default CustomToolbarSelectForUpdateAll;
