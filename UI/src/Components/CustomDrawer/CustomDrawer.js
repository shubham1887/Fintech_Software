import React, { Fragment } from 'react';
import { SwipeableDrawer, IconButton } from '@material-ui/core';
import CustomStyles from 'Components/CustomStyles';
import CloseIcon from '@material-ui/icons/Close';

function CustomDrawer({ IsOpen, openCallbak, children }) {
    const classes = CustomStyles();
    const [open, setOpen] = React.useState(IsOpen);

    const handleDrawerOpen = () => {
        openCallbak(open);
        setOpen(open);
    };
    const handleDrawerClose = () => {
        setOpen(false);
        openCallbak(false);
    };

    return (
        <Fragment key={'right'}>
            <SwipeableDrawer
                anchor={'right'}
                open={IsOpen}
                onClose={handleDrawerClose}
                onOpen={handleDrawerOpen}
            >
                <p className={classes.drawerclose}>
                    <IconButton
                        onClick={handleDrawerClose}
                        color="primary"
                        aria-label="close icon"
                        component="span"
                    >
                        <CloseIcon />
                    </IconButton>
                </p>

                {children}
            </SwipeableDrawer>
        </Fragment>
    );
}

export default CustomDrawer;
