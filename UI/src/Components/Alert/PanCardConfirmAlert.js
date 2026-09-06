import React, { useState, useEffect } from 'react';
import Button from '@material-ui/core/Button';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import DialogContentText from '@material-ui/core/DialogContentText';
import DialogTitle from '@material-ui/core/DialogTitle';
import { makeStyles } from '@material-ui/core/styles';
import {
    PRIMARY_COLOR,

} from 'Config';

const useStyles = makeStyles((theme) => ({
    customDialog: {
        maxWidth: '500px',
        width: '100%',
        color: PRIMARY_COLOR,
        font: '500 16px Roboto,RobotoDraft,Helvetica,Arial,sans-serif'
    },
    customButton: {
        color: 'white',
        backgroundColor: PRIMARY_COLOR,
        '&:hover': {
            backgroundColor: 'darken(PRIMARY_COLOR, 10%)',
        },
    },
}));

export function ConfirmAlertAPI({
    isOpen,
    title,
    message,
    request,
    positiveButtonTitle,
    positiveButtonClick,
    negativeButtonTitle,
    negativeButtonClick
}) {
    const [open, setOpen] = useState(isOpen);
    const classes = useStyles();

    useEffect(() => {
        setOpen(!!message);
    }, [message]);

    const handleClose = () => {
        setOpen(false);
        if (negativeButtonClick) {
            negativeButtonClick(false);
        }
    };

    return (
        <Dialog
            open={open}
            onClose={handleClose}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
            classes={{ paper: classes.customDialog }}
        >
            <DialogTitle id="alert-dialog-title">{title}</DialogTitle>
            <DialogContent>
                <DialogContentText id="alert-dialog-description">
                    {message}
                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Button
                    onClick={() => {
                        setOpen(false);
                        positiveButtonClick(request);
                    }}
                    className={classes.customButton}
                >
                    {positiveButtonTitle ? positiveButtonTitle : 'Yes'}
                </Button>
            </DialogActions>
        </Dialog>
    );
}
