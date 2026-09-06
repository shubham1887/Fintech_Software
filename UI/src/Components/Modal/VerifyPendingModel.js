import React, { useState, useEffect } from 'react';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Grid
} from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';

const useStyles = makeStyles((theme) => ({
    title: {
        fontWeight: 'bold',
        textAlign: 'center',
        padding: theme.spacing(2),
        color: '#fff'
    },
    dialogContent: {
        textAlign: 'center'
    },
    dialogActions: {
        display: 'flex',
        justifyContent: 'center',
        padding: theme.spacing(2)
    },
    button: {
        margin: theme.spacing(1),
        padding: theme.spacing(1, 6)
    },
    errorMessage: {
        color: 'red',
        fontWeight: 'bold'
    },
    successMessage: {
        color: 'green',
        fontWeight: 'bold'
    },
    subtitle: {
        marginTop: theme.spacing(2)
    }
}));

export function VerifyPendingModel({
    isOpen,
    title,
    status,
    CheckStatusClick,
    ProceedWithoutValidationClick,
    RetryClick // Added for handling retry action
}) {
    const classes = useStyles();
    const [open, setOpen] = useState(isOpen);
    const [bgColor, setBGColor] = useState('');

    useEffect(() => {
        switch (status) {
            case 'FAILURE':
            case 'REFUND':
                setBGColor('#ff0000');
                break;
            case 'PROCESS':
            case 'PENDING':
                setBGColor('#f58220');
                break;
            default:
                setBGColor('#ffffff'); 
        }
    }, [status]);

    const handleProceedWithoutValidationClick = () => {
        setOpen(false);
        ProceedWithoutValidationClick();
    };

    const handleCheckStatusClick = () => {
        setOpen(false);
        CheckStatusClick();
    };

    const handleRetryClick = () => {
        setOpen(false);
        RetryClick();
    };

    if (!isOpen) {
        return null;
    }

    return (
        <Dialog
            fullWidth
            maxWidth="sm"
            open={open}
            onClose={handleProceedWithoutValidationClick}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle
                id="alert-dialog-title"
                className={classes.title}
                style={{ background: bgColor }}
            >
                {title}
            </DialogTitle>
            <DialogContent className={classes.dialogContent}>
                <h2>Beneficiary Verification</h2>
                <p className={classes.errorMessage}>
                    {status === 'PROCESS'
                        ? 'Beneficiary verification is pending'
                        : 'Beneficiary verification failed'}
                </p>
            </DialogContent>
            <DialogActions className={classes.dialogActions}>
                <Grid container spacing={2} justifyContent="center">
                    {status === 'FAILURE' ? (
                        <Grid item>
                            <Button
                                onClick={handleRetryClick}
                                className={classes.button}
                                variant="contained"
                                color="primary"
                            >
                                Retry
                            </Button>
                        </Grid>
                    ) : (
                        <Grid item>
                            <Button
                                onClick={handleCheckStatusClick}
                                className={classes.button}
                                variant="contained"
                                color="primary"
                            >
                                Check Status
                            </Button>
                        </Grid>
                    )}
                    <Grid item>
                        <Button
                            onClick={handleProceedWithoutValidationClick}
                            className={classes.button}
                            variant="contained"
                            color="secondary"
                        >
                            Proceed without validation
                        </Button>
                    </Grid>
                </Grid>
            </DialogActions>
        </Dialog>
    );
}
