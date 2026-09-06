import React from 'react';
import IconButton from '@material-ui/core/IconButton';
import Tooltip from '@material-ui/core/Tooltip';
import AddIcon from '@material-ui/icons/Add';
import { withStyles } from '@material-ui/core/styles';

const defaultToolbarStyles = {
    iconButton: {}
};

const CustomToolbar = (props) => {
    const handleClick = () => {};

    return (
        <React.Fragment>
            <Tooltip title={props.Title}>
                <IconButton onClick={handleClick}>
                    <AddIcon />
                </IconButton>
            </Tooltip>
        </React.Fragment>
    );
};

export default withStyles(defaultToolbarStyles, { name: 'CustomToolbar' })(
    CustomToolbar
);
